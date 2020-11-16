namespace BusinessApp.Data.IntegrationTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.Domain;
    using BusinessApp.Test;
    using Microsoft.EntityFrameworkCore;
    using Xunit;
    using BusinessApp.App;
    using System.Threading;

    [Collection(nameof(DatabaseCollection))]
    public class EFDatastoreTests : IDisposable
    {
        private readonly BusinessAppDbContext db;
        private readonly ILinqSpecificationBuilder<IQuery, ResponseStub> linqBuilder;
        private readonly IQueryVisitorFactory<IQuery, ResponseStub> queryVisitorFactory;
        private readonly EFDatastore<ResponseStub> sut;

        public EFDatastoreTests(DatabaseFixture fixture)
        {
            linqBuilder = A.Fake<ILinqSpecificationBuilder<IQuery, ResponseStub>>();
            queryVisitorFactory = A.Fake<IQueryVisitorFactory<IQuery, ResponseStub>>();
            db = fixture.DbContext;
            sut = new EFDatastore<ResponseStub>(linqBuilder, queryVisitorFactory, db);
        }

        public void Dispose()
        {
            foreach (var entry in db.ChangeTracker.Entries<ResponseStub>())
            {
                entry.State = EntityState.Detached;
            }
        }

        public class Constructor : EFDatastoreTests
        {
            public Constructor(DatabaseFixture f) : base(f)
            {}

            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IQueryVisitorFactory<IQuery, ResponseStub>>(),
                    A.Dummy<BusinessAppDbContext>(),
                },
                new object[]
                {
                    A.Dummy<ILinqSpecificationBuilder<IQuery, ResponseStub>>(),
                    null,
                    A.Dummy<BusinessAppDbContext>(),
                },
                new object[]
                {
                    A.Dummy<ILinqSpecificationBuilder<IQuery, ResponseStub>>(),
                    A.Dummy<IQueryVisitorFactory<IQuery, ResponseStub>>(),
                    null
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                ILinqSpecificationBuilder<IQuery, ResponseStub> l,
                IQueryVisitorFactory<IQuery, ResponseStub> q,
                BusinessAppDbContext d)
            {
                /* Arrange */
                void shouldThrow() => new EFDatastore<ResponseStub>(l, q, d);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class QueryAsync : EFDatastoreTests
        {
            public QueryAsync(DatabaseFixture fixture): base(fixture)
            {
            }

            [Fact]
            public async Task LocalOrDbDoesNotExist_EmptyReturned()
            {
                /* Arrange */
                var queryVisitor = A.Fake<IQueryVisitor<ResponseStub>>();
                A.CallTo(() => linqBuilder.Build(A<Query>._))
                    .Returns(new NullSpecification<ResponseStub>(false));
                A.CallTo(() => queryVisitorFactory.Create(A<Query>._)).Returns(queryVisitor);
                A.CallTo(() => queryVisitor.Visit(A<DbSet<ResponseStub>>._))
                    .Returns(db.Set<ResponseStub>());

                /* Act */
                var dbAr = await sut.QueryAsync(A.Dummy<Query>(), A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Empty(dbAr);
            }

            [Fact]
            public async Task LocalCopyDoesNotExist_DbQueried()
            {
                /* Arrange */
                var query = new ResponseQueryStub();
                var queryVisitor = A.Fake<IQueryVisitor<ResponseStub>>();
                IEnumerable<ResponseStub> queryResults = null;
                A.CallTo(() => linqBuilder.Build(query))
                    .Returns(new NullSpecification<ResponseStub>(false));
                A.CallTo(() => queryVisitorFactory.Create(query)).Returns(queryVisitor);
                A.CallTo(() => queryVisitor.Visit(A<DbSet<ResponseStub>>._))
                    .Returns(db.Set<ResponseStub>().Where(r => r.Id == 111));

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    db.Add(new ResponseStub() { Id = 111 });
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.ResponseStub ON;");
                    db.SaveChanges();
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.ResponseStub OFF;");
                    queryResults = await sut.QueryAsync(query, A.Dummy<CancellationToken>());
                }

                /* Assert */
                Assert.Single(queryResults);
            }

            [Fact]
            public async Task LocalCopyExists_DbNotQueried()
            {
                /* Arrange */
                var query = new ResponseQueryStub();
                var response = new ResponseStub();
                IEnumerable<ResponseStub> queryResults = null;
                db.Add(response);
                A.CallTo(() => linqBuilder.Build(query))
                    .Returns(new NullSpecification<ResponseStub>(true));

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    db.SaveChanges();
                    queryResults = await sut.QueryAsync(query, A.Dummy<CancellationToken>());
                }

                /* Assert */
                Assert.Collection(queryResults, q => Assert.Same(response, q));
            }

            [Fact]
            public async Task LocalCopyExists_VisitorFactoryNotCalled()
            {
                /* Arrange */
                var query = new ResponseQueryStub();
                var response = new ResponseStub();
                IEnumerable<ResponseStub> queryResults = null;
                db.Add(response);
                A.CallTo(() => linqBuilder.Build(query))
                    .Returns(new NullSpecification<ResponseStub>(true));

                /* Act */
                using (var tran = db.Database.BeginTransaction())
                {
                    db.SaveChanges();
                    queryResults = await sut.QueryAsync(query, A.Dummy<CancellationToken>());
                }

                /* Assert */
                A.CallTo(() => queryVisitorFactory.Create(A<Query>._)).MustNotHaveHappened();;
            }
        }

        private sealed class ResponseQueryStub : Query
        {
            public override IEnumerable<string> Sort { get; set; } = new List<string>();
        }
    }
}
