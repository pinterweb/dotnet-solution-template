using System;
using System.Collections.Generic;
using System.Linq;
using BusinessApp.Kernel;
using BusinessApp.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BusinessApp.Infrastructure.Persistence.IntegrationTest
{
    [Collection(nameof(DatabaseCollection))]
    public class EFQueryVisitorTests : IDisposable
    {
        private readonly BusinessAppDbContext db;
        private readonly IEnumerable<ResponseStub> dataset;
        private EFQueryVisitor<ResponseStub> sut;

        public EFQueryVisitorTests(DbDatabaseFixture fixture)
        {
            this.db = fixture.DbContext;

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
            public Constructor(DbDatabaseFixture fixture) : base(fixture) {}

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
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class Visit : EFQueryVisitorTests
        {
            public Visit(DbDatabaseFixture fixture) : base(fixture) {}

            [Fact]
            public void UnknownEmbedFields_IgnoresThem()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Embed = new [] { "foobar" }
                };
                sut = new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var ex = Record.Exception(() => sut.Visit(db.Set<ResponseStub>()));

                /* Assert */
                Assert.Null(ex);
            }

            [Fact]
            public void UnknownExpandFields_IgnoresThem()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Expand = new [] { "foobar" }
                };
                sut = new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var ex = Record.Exception(() => sut.Visit(db.Set<ResponseStub>()));

                /* Assert */
                Assert.Null(ex);
            }

            [Fact]
            public void WithExpand_DataIncluded()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Expand = new [] { nameof(ResponseStub.ChildResponseStubs) }
                };
                sut = new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var newQuery = sut.Visit(db.Set<ResponseStub>());

                /* Assert - if not included children would be null */
                var queryResults = newQuery.ToList();
                Assert.Empty(queryResults.SelectMany(q => q.ChildResponseStubs));
            }

            [Fact]
            public void WithEmbed_DataIncluded()
            {
                /* Arrange */
                var query = new QueryStub
                {
                    Embed = new [] { nameof(ResponseStub.ChildResponseStubs) }
                };
                sut = new EFQueryVisitor<ResponseStub>(query);

                /* Act */
                var newQuery = sut.Visit(db.Set<ResponseStub>());

                /* Assert - if not included children would be null */
                var queryResults = newQuery.ToList();
                Assert.Empty(queryResults.SelectMany(q => q.ChildResponseStubs));
            }

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

        private sealed class QueryStub : Query
        {
            public override IEnumerable<string> Sort { get; set; } = new List<string>();
        }
    }
}
