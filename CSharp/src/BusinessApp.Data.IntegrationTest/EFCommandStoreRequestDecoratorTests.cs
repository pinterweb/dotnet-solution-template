namespace BusinessApp.Data.IntegrationTest
{
    using Xunit;
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using FakeItEasy;
    using System.Security.Principal;
    using BusinessApp.Test.Shared;
    using BusinessApp.App;
    using System.Threading;

    [Collection(nameof(DatabaseCollection))]
    public class EFCommandStoreRequestDecoratorTests
    {
        private readonly EFCommandStoreRequestDecorator<RequestStub, ResponseStub> sut;
        private readonly IRequestHandler<RequestStub, ResponseStub> inner;
        private readonly BusinessAppDbContext db;
        private readonly IPrincipal user;
        private readonly CancellationToken cancelToken;

        public EFCommandStoreRequestDecoratorTests(DatabaseFixture fixture)
        {
            user = A.Fake<IPrincipal>();
            inner = A.Fake<IRequestHandler<RequestStub, ResponseStub>>();
            db = A.Fake<BusinessAppDbContext>();
            cancelToken = A.Dummy<CancellationToken>();

            A.CallTo(() => user.Identity.Name).Returns("f");
        }


        public class Constructor : EFCommandStoreRequestDecoratorTests
        {
            public Constructor(DatabaseFixture f) : base(f)
            {}

            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[]
                        {
                            null,
                            A.Dummy<IPrincipal>(),
                            A.Dummy<BusinessAppDbContext>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<RequestStub, ResponseStub>>(),
                            null,
                            A.Dummy<BusinessAppDbContext>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<RequestStub, ResponseStub>>(),
                            A.Dummy<IPrincipal>(),
                            null
                        },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IRequestHandler<RequestStub, ResponseStub> i,
                IPrincipal p, BusinessAppDbContext db)
            {
                /* Arrange */
                void shouldThrow() => new EFCommandStoreRequestDecorator<RequestStub, ResponseStub>(i, p, db);

                /* Act */
                var ex = Record.Exception((Action)shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : EFCommandStoreRequestDecoratorTests
        {
            private readonly DatabaseFixture fixture;

            public HandleAsync(DatabaseFixture f) : base(f)
            {
                fixture = f;
            }

            [Fact]
            public void RequestMetadata_SetsRequestProperty()
            {
                /* Arrange */
                var request = A.Dummy<RequestStub>();
                RequestMetadata<RequestStub> metadata = null;
                A.CallTo(() => db.Add(A<RequestMetadata<RequestStub>>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<RequestMetadata<RequestStub>>(0));

                /* Act */
                var _ = sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Same(request, metadata.Request);
            }

            [Fact]
            public void RequestMetadata_SetsUsernameProperty()
            {
                /* Arrange */
                RequestMetadata<RequestStub> metadata = null;
                A.CallTo(() => db.Add(A<RequestMetadata<RequestStub>>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<RequestMetadata<RequestStub>>(0));

                /* Act */
                var _ = sut.HandleAsync(A.Dummy<RequestStub>(), cancelToken);

                /* Assert */
                Assert.Same("f", metadata.Username);
            }

            [Fact]
            public void RequestMetadata_OccurredUtcSet()
            {
                /* Arrange */
                RequestMetadata<RequestStub> metadata = null;
                A.CallTo(() => db.Add(A<RequestMetadata<RequestStub>>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<RequestMetadata<RequestStub>>(0));

                /* Act */
                var before = DateTimeOffset.UtcNow;
                var _ = sut.HandleAsync(A.Dummy<RequestStub>(), cancelToken);
                var after = DateTimeOffset.UtcNow;

                /* Assert */
                Assert.True(before <= metadata.OccurredUtc);
                Assert.True(after >= metadata.OccurredUtc);
            }

            [Fact]
            public void InnerHandle_CalledAfterDbAdd()
            {
                /* Arrange */
                var request = A.Dummy<RequestStub>();

                /* Act */
                var _ = sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => db.Add(A<RequestMetadata<RequestStub>>._))
                    .MustHaveHappenedOnceExactly()
                    .Then(
                        A.CallTo(() => inner.HandleAsync(request, cancelToken))
                            .MustHaveHappenedOnceExactly()
                    );
            }
        }
    }
}
