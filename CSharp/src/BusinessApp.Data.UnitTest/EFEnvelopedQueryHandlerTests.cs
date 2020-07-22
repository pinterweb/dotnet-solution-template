namespace BusinessApp.Data.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class EFEnvelopedQueryHandlerTests
    {
        private readonly BusinessAppReadOnlyDbContext db;
        private readonly IDbSetVisitorFactory<DummyRequest, DummyResponse> dbSetFactory;
        private readonly IQueryVisitorFactory<DummyRequest, DummyResponse> queryVisitorFactory;
        private EFEnvelopedQueryHandler<DummyRequest, DummyResponse> sut;

        public EFEnvelopedQueryHandlerTests()
        {
            db = A.Dummy<BusinessAppReadOnlyDbContext>();
            dbSetFactory = A.Fake<IDbSetVisitorFactory<DummyRequest, DummyResponse>>();
            queryVisitorFactory = A.Fake<IQueryVisitorFactory<DummyRequest, DummyResponse>>();

            sut = new EFEnvelopedQueryHandler<DummyRequest, DummyResponse>(db, queryVisitorFactory, dbSetFactory);
        }

        public class Constructor : EFEnvelopedQueryHandlerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IQueryVisitorFactory<DummyRequest, DummyResponse>>(),
                    A.Dummy<IDbSetVisitorFactory<DummyRequest, DummyResponse>>()
                },
                new object[]
                {
                    A.Dummy<BusinessAppReadOnlyDbContext>(),
                    null,
                    A.Dummy<IDbSetVisitorFactory<DummyRequest, DummyResponse>>()
                },
                new object[]
                {
                    A.Dummy<BusinessAppReadOnlyDbContext>(),
                    A.Dummy<IQueryVisitorFactory<DummyRequest, DummyResponse>>(),
                    null
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(BusinessAppReadOnlyDbContext r,
                IQueryVisitorFactory<DummyRequest, DummyResponse> q,
                IDbSetVisitorFactory<DummyRequest, DummyResponse> d)
            {
                /* Arrange */
                void shouldThrow() => new EFEnvelopedQueryHandler<DummyRequest, DummyResponse>(r, q, d);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class HandleAsync : EFEnvelopedQueryHandlerTests
        {
            private readonly IQueryable<DummyResponse> results;
            private readonly DummyRequest query;
            private readonly IDbSetVisitor<DummyResponse> dbSetVisitor;
            private readonly IQueryVisitor<DummyResponse> queryVisitor;

            public HandleAsync()
            {
                query = new DummyRequest();

                dbSetVisitor = A.Fake<IDbSetVisitor<DummyResponse>>();
                queryVisitor = A.Fake<IQueryVisitor<DummyResponse>>();
                A.CallTo(() => dbSetFactory.Create(query)).Returns(dbSetVisitor);
                A.CallTo(() => queryVisitorFactory.Create(query)).Returns(queryVisitor);

                results = new[]
                {
                    new DummyResponse(),
                    new DummyResponse(),
                    new DummyResponse()
                }.AsQueryable();

                A.CallTo(() => dbSetVisitor.Visit(A<DbSet<DummyResponse>>._))
                    .Returns(results);
                A.CallTo(() => queryVisitor.Visit(A<IQueryable<DummyResponse>>._))
                    .Returns(new FakeQueryable<DummyResponse>(results));
            }

            [Theory]
            [InlineData(0)]
            [InlineData(null)]
            public async Task WithNoQueryParams_ReturnsAll(int? offset)
            {
                /* Arrange */
                query.Offset = offset;

                /* Act */
                var returnedData = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(3, returnedData.Data.Count());
            }

            [Fact]
            public async Task WithOffset_LimitsData()
            {
                /* Arrange */
                query.Offset = 1;

                /* Act */
                var returnedData = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(2, returnedData.Data.Count());
            }

            [Fact]
            public async Task WithLimit_FirstTaken()
            {
                /* Arrange */
                query.Limit = 1;

                /* Act */
                var returnedData = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Same(results.First(), Assert.Single(returnedData.Data));
            }

            [Fact]
            public async Task WithLimitAndOffset_DatasetFiltered()
            {
                /* Arrange */
                query.Limit = 1;
                query.Offset = 2;

                /* Act */
                var returnedData = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Same(results.Last(), Assert.Single(returnedData.Data));
            }

            [Fact]
            public async Task WithLimitAndOffset_TotalCountNotAffected()
            {
                /* Arrange */
                query.Limit = 1;
                query.Offset = 2;

                /* Act */
                var returnedData = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(3, returnedData.Pagination.ItemCount);
            }
        }
    }
}
