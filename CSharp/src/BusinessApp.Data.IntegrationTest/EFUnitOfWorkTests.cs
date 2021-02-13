namespace BusinessApp.Data.IntegrationTest
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using BusinessApp.Domain;
    using Xunit;
    using BusinessApp.Test;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Data;

    [Collection(nameof(DatabaseCollection))]
    public class EFUnitOfWorkTests : IDisposable
    {
        private readonly IUnitOfWork inner;
        private readonly BusinessAppDbContext db;
        private readonly EFUnitOfWork sut;

        public EFUnitOfWorkTests(DatabaseFixture fixture)
        {
            inner = A.Fake<IUnitOfWork>();
            db = fixture.DbContext;
            sut = new EFUnitOfWork(db, inner);
        }

        public virtual void Dispose()
        {
            foreach (var entry in db.ChangeTracker.Entries<ResponseStub>())
            {
                entry.State = EntityState.Detached;
            }
        }

        public class Constructor : EFUnitOfWorkTests
        {
            public Constructor(DatabaseFixture fixture)
                :base(fixture)
            { }

            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new[]
                    {
                        new object[]
                        {
                            null,
                            A.Dummy<EventUnitOfWork>(),
                        },
                        new object[]
                        {
                            A.Dummy<BusinessAppDbContext>(),
                            null
                        },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidArgs_ExceptionThrown(BusinessAppDbContext d,
                EventUnitOfWork p)
            {
                /* Arrange */
                void shouldThrow() => new EFUnitOfWork(d, p);

                /* Act */
                var ex = Record.Exception((Action)shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class Track : EFUnitOfWorkTests
        {
            public Track(DatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void AggregateRoot_InnerUnitOfWorkTrackCalled()
            {
                /* Arrange */
                var ar = A.Dummy<AggregateRoot>();

                /* Act */
                sut.Track(ar);

                /* Assert */
                A.CallTo(() => inner.Track(ar)).MustHaveHappenedOnceExactly();
            }
        }

        public class Add : EFUnitOfWorkTests
        {
            public Add(DatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void AggregateRoot_InnerUnitOfWorkAddCalled()
            {
                /* Arrange */
                var ar = A.Dummy<AggregateRoot>();

                /* Act */
                sut.Add(ar);

                /* Assert */
                A.CallTo(() => inner.Add(ar)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void IDomainEvent_InnerUnitOfWorkAddCalled()
            {
                /* Arrange */
                var e = A.Dummy<IDomainEvent>();

                /* Act */
                sut.Add(e);

                /* Assert */
                A.CallTo(() => inner.Add(e)).MustHaveHappenedOnceExactly();
            }
        }

        public class Remove : EFUnitOfWorkTests
        {
            public Remove(DatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void AggregateRoot_InnerUnitOfWorkRemoveC()
            {
                /* Arrange */
                var ar = A.Dummy<AggregateRoot>();

                /* Act */
                sut.Remove(ar);

                /* Assert */
                A.CallTo(() => inner.Remove(ar)).MustHaveHappenedOnceExactly();
            }
        }

        public class CommitAsync : EFUnitOfWorkTests
        {
            private readonly CancellationToken token;

            public CommitAsync(DatabaseFixture fixture)
                :base(fixture)
            {
                token = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task CommittingEvent_CalledBeforeInnerUow()
            {
                /* Arrange */
                int innerCalls = 1;
                sut.Committing += (sender, args) =>
                {
                    innerCalls = Fake.GetCalls(inner).Count();
                };

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.CommitAsync(token);
                }

                /* Assert */
                Assert.Equal(0, innerCalls);
            }

            [Fact]
            public async Task InnerUnitOfWork_CalledBeforeDbContextSaveChanges()
            {
                /* Arrange */
                EntityState? stateDuringEvents = null;
                var entity = new ResponseStub();
                db.Add(entity);

                A.CallTo(() => inner.CommitAsync(token))
                    .Invokes(ctx =>
                    {
                        stateDuringEvents = db.Entry(entity).State;
                    });

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.CommitAsync(token);
                }

                /* Assert - Entity would be unchanged if called after SaveChanges */
                Assert.Equal(EntityState.Added, stateDuringEvents);
            }

            [Fact]
            public async Task OnConcurrencyException_WrapsDbUpdateConcurrencyException()
            {
                /* Arrange */
                var entity = new ResponseStub();
                db.Add(entity);
                db.SaveChanges();
                db.ChangeTracker.Entries<ResponseStub>()
                    .Single(e => e.Entity == entity)
                    .State = EntityState.Detached;
                var dbEntity = db.Set<ResponseStub>().SingleOrDefault(m => m.Id == entity.Id);
                db.ChangeTracker.Entries<ResponseStub>()
                    .Single(e => e.Entity == dbEntity)
                    .State = EntityState.Deleted;

                /* Act */
                sut.Committing += (sender, args) =>
                {
                    db.Database.ExecuteSqlInterpolated(
                        $"delete from dbo.ResponseStub where Id = {entity.Id}"
                    );
                };
                var exception = await Record.ExceptionAsync(() => sut.CommitAsync(token));

                /* Assert */
                var dbEx = Assert.IsType<DBConcurrencyException>(exception);
                Assert.Equal(
                    "An error occurred while saving your data. " +
                    "The data may have been modified or deleted while you were working " +
                    "Please make sure you are working with the most up to date data",
                    dbEx.Message
                );
            }

            [Fact]
            public async Task HasExternalTranscation_DoesNotCommit()
            {
                /* Arrange */
                var entity = new ResponseStub();
                db.Add(entity);

                /* Act */
                using (var trans = db.Database.BeginTransaction())
                {
                    await sut.CommitAsync(A.Dummy<CancellationToken>());
                    trans.Rollback();
                }

                /* Assert */
                Assert.Empty(
                    db.Set<ResponseStub>().Where(e => e.Id == entity.Id)
                );
            }

            [Fact]
            public async Task AfterDbContextSaveChanges_CommittedEventInvoked()
            {
                /* Arrange */
                EntityState? stateDuringEvent = null;
                var entity = new ResponseStub();
                db.Add(entity);
                sut.Committed += (sender, args) =>
                {
                    stateDuringEvent = db.Entry(entity).State;
                };

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.CommitAsync(token);
                }

                /* Assert */
                Assert.Equal(EntityState.Unchanged, stateDuringEvent);
            }
        }

        public class Begin : EFUnitOfWorkTests, IDisposable
        {
            public Begin(DatabaseFixture fixture)
                :base(fixture)
            {}

            public override void Dispose()
            {
                if (db.Database.CurrentTransaction != null)
                {
                    db.Database.CurrentTransaction.Rollback();
                }
                base.Dispose();
            }

            [Fact]
            public void UsingInnerDatabase_BeginsTransaction()
            {
                /* Act */
                sut.Begin();

                /* Assert */
                Assert.NotNull(db.Database.CurrentTransaction);
            }

            [Fact]
            public void AsIUnitOfWork_ReturnsSelf()
            {
                /* Act */
                var uow = sut.Begin();

                /* Assert */
                Assert.Same(uow, sut);
            }

            [Fact]
            public async Task OnCommit_CommitTransaction()
            {
                /* Arrange */
                var uow = sut.Begin();

                /* Act */
                await uow.CommitAsync(A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Null(db.Database.CurrentTransaction);
            }
        }

        public class RevertAsync : EFUnitOfWorkTests
        {
            private readonly CancellationToken token;

            public RevertAsync(DatabaseFixture fixture)
                :base(fixture)
            {
                token = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task CommittingEvent_IsNotCalled()
            {
                /* Arrange */
                bool committingCalled = false;
                sut.Committing += (sender, args) => committingCalled = true;

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.RevertAsync(token);
                }

                /* Assert */
                Assert.False(committingCalled);
            }

            [Fact]
            public async Task BeforeSaveChanges_InnerRevertAsyncCalled()
            {
                /* Arrange */
                EntityState? stateDuringEvent = null;
                var entity = new ResponseStub();
                var entry = db.Add(entity);
                A.CallTo(() => inner.RevertAsync(token))
                    .Invokes(ctx => stateDuringEvent = entry.State);

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.RevertAsync(token);
                }

                /* Assert */
                Assert.Equal(EntityState.Added, stateDuringEvent);
            }

            [Fact]
            public async Task HasExternalTranscation_DoesNotCommit()
            {
                /* Arrange */
                var entity = new ResponseStub();
                db.Add(entity);

                /* Act */
                using (var trans = db.Database.BeginTransaction())
                {
                    await sut.RevertAsync(A.Dummy<CancellationToken>());
                    trans.Rollback();
                }

                /* Assert */
                Assert.Empty(
                    db.Set<ResponseStub>().Where(e => e.Id == entity.Id)
                );
            }

            [Fact]
            public async Task AfterSave_SavedEventCalled()
            {
                /* Arrange */
                bool fired = false;
                ResponseStub addedEntity = null;
                var entity = new ResponseStub();
                db.Add(entity);
                sut.Committed += (sender, args) =>
                {
                    fired = true;
                    addedEntity = db.Set<ResponseStub>().SingleOrDefault(p => p.Id == entity.Id);
                };

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.RevertAsync(token);
                }

                /* Assert */
                Assert.True(fired);
                Assert.NotNull(addedEntity);
            }
        }

        public class Find : EFUnitOfWorkTests
        {
            public Find(DatabaseFixture fixture)
                :base(fixture)
            {}

            [Fact]
            public void ReturnsAllTypedAggregateRoots()
            {
                /* Arrange */
                var ar1 = new ARStub();
                var ar2 = new ARStub();
                var other = A.Dummy<AnotherARStub>();
                sut.Track(ar1);
                sut.Track(ar2);
                sut.Track(other);

                /* Act */
                var instance = sut.Find<ARStub>(a => a.Equals(ar1));

                /* Assert */
                Assert.Same(ar1, instance);
            }

            private sealed class ARStub : AggregateRoot
            {
                public int Id { get; set; }
            }

            private sealed class AnotherARStub : AggregateRoot
            {
                public int Id { get; set; }
            }
        }
    }
}
