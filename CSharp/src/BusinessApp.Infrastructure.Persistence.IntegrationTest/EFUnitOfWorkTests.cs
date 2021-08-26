using System;
using System.Collections.Generic;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using BusinessApp.Kernel;
using Xunit;
using BusinessApp.Test.Shared;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Data;

namespace BusinessApp.Infrastructure.Persistence.IntegrationTest
{
    [Collection(nameof(DatabaseCollection))]
    public class EFUnitOfWorkTests : IDisposable
    {
        private readonly BusinessAppDbContext db;
        private readonly EFUnitOfWork sut;

        public EFUnitOfWorkTests(DbDatabaseFixture fixture)
        {
            db = fixture.DbContext;
            sut = new EFUnitOfWork(db);
        }

        public virtual void Dispose()
        {
            db.ChangeTracker.Clear();
        }

        public class Constructor : EFUnitOfWorkTests
        {
            public Constructor(DbDatabaseFixture fixture)
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
                        },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidArgs_ExceptionThrown(BusinessAppDbContext d)
            {
                /* Arrange */
                void shouldThrow() => new EFUnitOfWork(d);

                /* Act */
                var ex = Record.Exception((Action)shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class Track : EFUnitOfWorkTests
        {
            public Track(DbDatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void EntityStateUnchanged()
            {
                /* Arrange */
                var ar = new AggregateRootStub() { Id = 1 };

                /* Act */
                sut.Track(ar);

                /* Assert */
                Assert.Equal(EntityState.Unchanged, db.Entry(ar).State);
            }
        }

        public class Add : EFUnitOfWorkTests
        {
            public Add(DbDatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void EntityStateAdded()
            {
                /* Arrange */
                var ar = new AggregateRootStub();

                /* Act */
                sut.Add(ar);

                /* Assert */
                Assert.Equal(EntityState.Added, db.Entry(ar).State);
            }
        }

        public class Remove : EFUnitOfWorkTests
        {
            public Remove(DbDatabaseFixture fixture)
                :base(fixture)
            { }

            [Fact]
            public void EntityStateDeleted()
            {
                /* Arrange */
                var ar = new AggregateRootStub() { Id = 44 };

                /* Act */
                sut.Remove(ar);

                /* Assert */
                Assert.Equal(EntityState.Deleted, db.Entry(ar).State);
            }
        }

        public class CommitAsync : EFUnitOfWorkTests
        {
            private readonly CancellationToken cancelToken;

            public CommitAsync(DbDatabaseFixture fixture)
                :base(fixture)
            {
                cancelToken = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task CommittingEvent_CalledBeforeSaveChanges()
            {
                /* Arrange */
                var ar = new AggregateRootStub();
                var state = EntityState.Unchanged;
                sut.Add(ar);
                sut.Committing += (sender, args) =>
                {
                    state = db.Entry(ar).State;
                };

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    await sut.CommitAsync(cancelToken);
                }

                /* Assert */
                Assert.Equal(EntityState.Added, state);
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
                var exception = await Record.ExceptionAsync(() => sut.CommitAsync(cancelToken));

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
                    await sut.CommitAsync(cancelToken);
                }

                /* Assert */
                Assert.Equal(EntityState.Unchanged, stateDuringEvent);
            }
        }

        public class Begin : EFUnitOfWorkTests, IDisposable
        {
            public Begin(DbDatabaseFixture fixture)
                :base(fixture)
            { }

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
            public void NoError_WrapsSelf()
            {
                /* Act */
                var uow = sut.Begin();

                /* Assert */
                Assert.Equal(uow, Result.Ok<IUnitOfWork>(sut));
            }

            [Fact]
            public async Task OnCommit_CommitTransaction()
            {
                /* Arrange */
                var uow = sut.Begin().Unwrap();

                /* Act */
                await uow.CommitAsync(A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Null(db.Database.CurrentTransaction);
            }

            [Fact]
            public void AlreadyInTransaction_ErrorKindReturned()
            {
                /* Arrange */
                db.Database.BeginTransaction();

                /* Act */
                var uow = sut.Begin();

                /* Assert */
                Assert.Equal(ValueKind.Error, uow.Kind);
            }

            [Fact]
            public void AlreadyInTransaction_InvalidOperationExceptionReturn()
            {
                /* Arrange */
                db.Database.BeginTransaction();

                /* Act */
                var uow = sut.Begin();

                /* Assert */
                Assert.IsType<InvalidOperationException>(uow.UnwrapError());
            }

            [Fact]
            public void AnyOtherError_Throws()
            {
                /* Arrange */
                var options = new DbContextOptionsBuilder<BusinessAppDbContext>()
                    .Options;
                var sutDb = new BusinessAppTestDbContext(db, options);
                var anotherSut = new EFUnitOfWork(sutDb);
                sutDb.Dispose();

                /* Act */
                var ex = Record.Exception(() => anotherSut.Begin());

                /* Assert */
                Assert.NotNull(ex);
            }
        }

        public class RevertAsync : EFUnitOfWorkTests
        {
            private readonly CancellationToken cancelToken;

            public RevertAsync(DbDatabaseFixture fixture)
                :base(fixture)
            {
                cancelToken = A.Dummy<CancellationToken>();
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
                    await sut.RevertAsync(cancelToken);
                }

                /* Assert */
                Assert.False(committingCalled);
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
                    await sut.RevertAsync(cancelToken);
                }

                /* Assert */
                Assert.True(fired);
                Assert.NotNull(addedEntity);
            }
        }

        public class Find : EFUnitOfWorkTests
        {
            public Find(DbDatabaseFixture fixture)
                :base(fixture)
            {}

            [Fact]
            public void ReturnsAllTypedAggregateRoots()
            {
                /* Arrange */
                var ar1 = new AggregateRootStub();
                db.Add(ar1);
                Func<AggregateRootStub, bool> filter = a => a.Equals(ar1);

                /* Act */
                var instance = sut.Find<AggregateRootStub>(filter);

                /* Assert */
                Assert.Same(ar1, instance);
            }
        }
    }
}
