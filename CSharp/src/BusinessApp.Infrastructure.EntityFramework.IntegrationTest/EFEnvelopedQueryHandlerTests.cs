using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using BusinessApp.Test.Shared;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BusinessApp.Infrastructure.EntityFramework.IntegrationTest
{
    [Collection(nameof(DatabaseCollection))]
    public class EFEnvelopedQueryHandlerTests : IDisposable
    {
        private readonly BusinessAppDbContext db;
        private readonly IDbSetVisitorFactory<EnvelopeRequestStub, ResponseStub> dbSetFactory;
        private readonly IQueryVisitorFactory<EnvelopeRequestStub, ResponseStub> queryVisitorFactory;
        private EFEnvelopedQueryHandler<EnvelopeRequestStub, ResponseStub> sut;
        private readonly IEnumerable<ResponseStub> dataset;

        public EFEnvelopedQueryHandlerTests(DatabaseFixture fixture)
        {
            db = fixture.DbContext;
            dbSetFactory = A.Fake<IDbSetVisitorFactory<EnvelopeRequestStub, ResponseStub>>();
            queryVisitorFactory = A.Fake<IQueryVisitorFactory<EnvelopeRequestStub, ResponseStub>>();

            sut = new EFEnvelopedQueryHandler<EnvelopeRequestStub, ResponseStub>(db,
                queryVisitorFactory,
                dbSetFactory);

            dataset = new[]
            {
                new ResponseStub(),
                new ResponseStub(),
                new ResponseStub()
            };

            db.AddRange(dataset);
            db.SaveChanges();
        }

        public void Dispose()
        {
            db.RemoveRange(dataset);
            db.SaveChanges();
        }

        public class Constructor : EFEnvelopedQueryHandlerTests
        {
            public Constructor(DatabaseFixture fixture) : base(fixture) {}

            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IQueryVisitorFactory<EnvelopeRequestStub, ResponseStub>>(),
                    A.Dummy<IDbSetVisitorFactory<EnvelopeRequestStub, ResponseStub>>()
                },
                new object[]
                {
                    A.Dummy<BusinessAppDbContext>(),
                    null,
                    A.Dummy<IDbSetVisitorFactory<EnvelopeRequestStub, ResponseStub>>()
                },
                new object[]
                {
                    A.Dummy<BusinessAppDbContext>(),
                    A.Dummy<IQueryVisitorFactory<EnvelopeRequestStub, ResponseStub>>(),
                    null
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(BusinessAppDbContext r,
                IQueryVisitorFactory<EnvelopeRequestStub, ResponseStub> q,
                IDbSetVisitorFactory<EnvelopeRequestStub, ResponseStub> d)
            {
                /* Arrange */
                void shouldThrow() => new EFEnvelopedQueryHandler<EnvelopeRequestStub, ResponseStub>(r, q, d);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : EFEnvelopedQueryHandlerTests
        {
            private readonly EnvelopeRequestStub query;

            public HandleAsync(DatabaseFixture fixture) : base(fixture)
            {
                query = new EnvelopeRequestStub();
                var dbSetVisitor = A.Fake<IDbSetVisitor<ResponseStub>>();
                DbSet<ResponseStub> originalDbSet = null;

                A.CallTo(() => dbSetFactory.Create(query)).Returns(dbSetVisitor);
                A.CallTo(() => dbSetVisitor.Visit(A<DbSet<ResponseStub>>._))
                    .Invokes(ctx => ctx.GetArgument<DbSet<ResponseStub>>(0))
                    .Returns(originalDbSet);

                dbSetVisitor = A.Fake<IDbSetVisitor<ResponseStub>>();
                var queryVisitor = A.Fake<IQueryVisitor<ResponseStub>>();
                A.CallTo(() => dbSetFactory.Create(query)).Returns(dbSetVisitor);
                A.CallTo(() => queryVisitorFactory.Create(query)).Returns(queryVisitor);

                A.CallTo(() => queryVisitor.Visit(A<IQueryable<ResponseStub>>._))
                    .Returns(db.Set<ResponseStub>());
            }

            [Theory]
            [InlineData(0)]
            [InlineData(null)]
            public async Task WithNoQueryParams_ReturnsAllData(int? offset)
            {
                /* Arrange */
                query.Offset = offset;

                /* Act */
                var result = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(3, result.Unwrap().Data.Count());
            }

            [Fact]
            public async Task WithLimitValue_ReturnsThatRecordCount()
            {
                /* Arrange */
                query.Limit = 2;

                /* Act */
                var result = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(2, result.Unwrap().Data.Count());
            }

            [Fact]
            public async Task WithOffset_OneRecordSkipped()
            {
                /* Arrange */
                query.Offset = 1;

                /* Act */
                var result = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(2, result.Unwrap().Data.Count());
            }

            [Fact]
            public async Task WithLimitAndOffset_TotalCountNotAffected()
            {
                /* Arrange */
                query.Limit = 2;
                query.Offset = 2;

                /* Act */
                var result = await sut.HandleAsync(query, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Equal(3, result.Unwrap().Pagination.ItemCount);
            }
        }
    }
}
