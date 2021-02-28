namespace BusinessApp.Data.IntegrationTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using BusinessApp.Test.Shared;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    [Collection(nameof(DatabaseCollection))]
    public class EFQuerySortVisitorTests : IDisposable
    {
        private readonly BusinessAppDbContext db;
        private readonly IEnumerable<ResponseStub> dataset;
        private EFQuerySortVisitor<ResponseStub> sut;

        public EFQuerySortVisitorTests(DatabaseFixture fixture)
        {
            this.db = fixture.DbContext;

            using (var transaction = db.Database.BeginTransaction())
            {
                dataset = new[]
                {
                    new ResponseStub() { Id = 5 },
                    new ResponseStub() { Id = 4 },
                    new ResponseStub() { Id = 6 },
                };
                db.AddRange(dataset);
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.ResponseStub ON;");
                db.SaveChanges();
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.ResponseStub OFF;");
                transaction.Commit();
            }
        }

        public void Dispose()
        {
            db.RemoveRange(dataset);
            db.SaveChanges();
        }

        public class Constructor : EFQuerySortVisitorTests
        {
            public Constructor(DatabaseFixture fixture) : base(fixture) {}

            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(Query query)
            {
                /* Arrange */
                void shouldThrow() => new EFQuerySortVisitor<ResponseStub>(query);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class Visit : EFQuerySortVisitorTests
        {
            public Visit(DatabaseFixture fixture) : base(fixture) {}

            [Fact]
            public void UnknownFields_IgnoresThem()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Sort = new [] { "foobar" }
                };
                sut = new EFQuerySortVisitor<ResponseStub>(query);

                /* Act */
                var ex = Record.Exception(() => sut.Visit(db.Set<ResponseStub>()));

                /* Assert - if not included children would be null */
                Assert.Null(ex);
            }

            [Fact]
            public void SortWithoutDescendingIndicator_SortedAscending()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Sort = new [] { nameof(ResponseStub.Id) }
                };
                sut = new EFQuerySortVisitor<ResponseStub>(query);

                /* Act */
                var newQuery = sut.Visit(db.Set<ResponseStub>());

                /* Assert - if not included children would be null */
                Assert.Collection(newQuery.ToList(),
                    q => Assert.Equal(4, q.Id),
                    q => Assert.Equal(5, q.Id),
                    q => Assert.Equal(6, q.Id)
                );
            }

            [Fact]
            public void SortWithDescendingIndicator_SortedDescending()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Sort = new [] { $"-{nameof(ResponseStub.Id)}" }
                };
                sut = new EFQuerySortVisitor<ResponseStub>(query);

                /* Act */
                var newQuery = sut.Visit(db.Set<ResponseStub>());

                /* Assert - if not included children would be null */
                Assert.Collection(newQuery.ToList(),
                    q => Assert.Equal(6, q.Id),
                    q => Assert.Equal(5, q.Id),
                    q => Assert.Equal(4, q.Id)
                );
            }
        }

        private sealed class QueryStub : Query
        {
            public override IEnumerable<string> Sort { get; set; } = new List<string>();
        }
    }
}
