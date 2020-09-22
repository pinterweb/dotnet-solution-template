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
        private readonly EventUnitOfWork inner;
        private readonly BusinessAppDbContext db;
        private readonly EFUnitOfWork sut;

        public EFUnitOfWorkTests(DatabaseFixture fixture)
        {
            inner = A.Fake<EventUnitOfWork>();
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

        public class UnitofWorkTrack : EFUnitOfWorkTests
        {
            public UnitofWorkTrack(DatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void OnTrack_EventUnitOfWorkTrackCalled()
            {
                /* Arrange */
                var ar = A.Dummy<AggregateRoot>();

                /* Act */
                sut.Track(ar);

                /* Assert */
                A.CallTo(() => inner.Track(ar)).MustHaveHappenedOnceExactly();
            }
        }

        public class UnitofWorkAdd : EFUnitOfWorkTests
        {
            public UnitofWorkAdd(DatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void OnAdd_EventUnitOfWorkAddCalled()
            {
                /* Arrange */
                var ar = A.Dummy<AggregateRoot>();

                /* Act */
                sut.Add(ar);

                /* Assert */
                A.CallTo(() => inner.Add(ar)).MustHaveHappenedOnceExactly();
            }
            }

        public class UnitofWorkRemove : EFUnitOfWorkTests
        {
            public UnitofWorkRemove(DatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void OnAdd_EventUnitOfWorkRemoveC()
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
            public async Task BeforeSave_EventUnitOfWorkCalled()
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

                /* Assert */
                Assert.Equal(EntityState.Added, stateDuringEvents);
            }

            [Fact]
            public async Task BeforeSaveChanges_CommittingEventInvoked()
            {
                /* Arrange */
                EntityState? stateDuringEvent = null;
                var entity = new ResponseStub();
                db.Add(entity);
                sut.Committing += (sender, args) =>
                {
                    stateDuringEvent = db.Entry(entity).State;
                };

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.CommitAsync(token);
                }

                /* Assert */
                Assert.Equal(EntityState.Added, stateDuringEvent);
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
            public async Task ExternalTranscation_DoesNotCommit()
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
            public async Task AfterSaveChanges_CommittedEventInvoked()
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
            public void WithDatabase_BeginsTransaction()
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
            public async Task OnSaveChanges_Commits()
            {
                /* Arrange */
                var uow = sut.Begin();

                /* Act */
                await sut.CommitAsync(A.Dummy<CancellationToken>());

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
            public async Task WithInnerEventUnit_EventUnitOfWorkReverted()
            {
                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.RevertAsync(token);
                }

                /* Assert */
                A.CallTo(() => inner.RevertAsync(token)).MustHaveHappenedOnceExactly();
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
    }
}
