namespace BusinessApp.Data.IntegrationTest
{
    using System;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using BusinessApp.Domain;
    using Xunit;
    using BusinessApp.Test;
    using System.Linq;

    [Collection(nameof(DatabaseCollection))]
    public class BusinessAppDbContextTests : IDisposable
    {
        private readonly EventUnitOfWork inner;
        private readonly BusinessAppDbContext sut;

        public BusinessAppDbContextTests(DatabaseFixture fixture)
        {
            inner = A.Fake<EventUnitOfWork>();
            sut = fixture.DbContext;
        }

        public virtual void Dispose()
        {
            foreach (var entry in sut.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
        }

        public class AddOrReplace : BusinessAppDbContextTests
        {
            private readonly IDatabase dbSut;

            public AddOrReplace(DatabaseFixture fixture)
                :base(fixture)
            {
                dbSut = sut;
            }

            [Fact]
            public void ExistsInDb_MarkedAsModified()
            {
                /* Arrange */
                var entity = new ResponseStub();
                sut.Add(entity);
                sut.SaveChanges();
                sut.Entry(entity).State = EntityState.Detached;
                var newEntity = new ResponseStub { Id = entity.Id };

                /* Act */
                dbSut.AddOrReplace(newEntity);

                /* Assert */
                Assert.Equal(EntityState.Modified, sut.Entry(newEntity).State);
            }

            [Fact]
            public void DoesNotExistInDb_MarkedAsAdded()
            {
                /* Arrange */
                var entity = new ResponseStub();

                /* Act */
                dbSut.AddOrReplace(entity);

                /* Assert */
                Assert.Equal(EntityState.Added, sut.Entry(entity).State);
            }
        }

        public class Remove : BusinessAppDbContextTests
        {
            private readonly IDatabase dbSut;

            public Remove(DatabaseFixture fixture)
                :base(fixture)
            {
                dbSut = sut;
            }

            [Fact]
            public void ByDefault_MarksEntityForRemoval()
            {
                /* Arrange */
                var entity = new ResponseStub();
                sut.Add(entity);
                sut.SaveChanges();

                /* Act */
                dbSut.Remove(entity);

                /* Assert */
                Assert.Equal(EntityState.Deleted, sut.Entry(entity).State);
            }
        }
    }
}
