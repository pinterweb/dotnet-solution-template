namespace BusinessApp.Data.IntegrationTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.App;
    using BusinessApp.Test;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    [Collection(nameof(DatabaseCollection))]
    public class EFQueryVisitorTests : IDisposable
    {
        private readonly BusinessAppReadOnlyDbContext db;
        private readonly IEnumerable<ResponseStub> dataset;
        private EFQueryVisitor<ResponseStub> sut;

        public EFQueryVisitorTests(DatabaseFixture fixture)
        {
            this.db = fixture.ReadContext;

            using (var transaction = db.Database.BeginTransaction())
            {
                dataset = new[]
                {
                    new ResponseStub() { Id = 1 },
                    new ResponseStub() { Id = 2 },
                    new ResponseStub() { Id = 3 },
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

        public class Constructor : EFQueryVisitorTests
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
                void shouldThrow() => new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class Visit : EFQueryVisitorTests
        {
            public Visit(DatabaseFixture fixture) : base(fixture) {}

            [Theory]
            [InlineData(0)]
            [InlineData(null)]
            public void WithNoQueryParams_ReturnsAllData(int? offset)
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Offset = offset
                };
                sut = new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var newQuery = sut.Visit(db.Set<ResponseStub>());

                /* Assert */
                var queryResults = newQuery.ToList();
                Assert.Equal(3, queryResults.Count());
            }

            [Fact]
            public void WithLimitValue_ReturnsThatRecordCount()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Limit = 2
                };
                sut = new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var newQuery = sut.Visit(db.Set<ResponseStub>());

                /* Assert */
                var queryResults = newQuery.ToList();
                Assert.Equal(2, queryResults.Count());
            }

            [Fact]
            public void WithOffset_OneRecordSkipped()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Offset = 1
                };
                sut = new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var newQuery = sut.Visit(db.Set<ResponseStub>());

                /* Assert */
                var queryResults = newQuery.ToList();
                Assert.Equal(2, queryResults.Count());
            }

            [Fact]
            public void WithOffsetAndLimit_CombinesTheTwo()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Offset = 1,
                    Limit = 2
                };
                sut = new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var newQuery = sut.Visit(db.Set<ResponseStub>());

                /* Assert */
                var queryResults = newQuery.OrderBy(q => q.Id).ToList();
                Assert.Collection(queryResults,
                    q => Assert.Equal(2, q.Id),
                    q => Assert.Equal(3, q.Id)
                );
            }
        }

        private sealed class QueryStub : Query {  }
    }
}
