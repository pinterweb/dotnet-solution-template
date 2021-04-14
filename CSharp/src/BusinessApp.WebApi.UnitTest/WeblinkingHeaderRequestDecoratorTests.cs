namespace BusinessApp.WebApi.UnitTest
{
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;
    using Xunit;
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.Extensions.Primitives;

    using HandlerResponse = HandlerContext<RequestStub, ResponseStub>;

    public class WeblinkingHeaderRequestDecoratorTests
    {
        private readonly IHttpRequestHandler<RequestStub, ResponseStub> inner;
        private IEnumerable<HateoasLink<RequestStub, ResponseStub>> links;
        private WeblinkingHeaderRequestDecorator<RequestStub, ResponseStub> sut;

        public WeblinkingHeaderRequestDecoratorTests()
        {
            inner = A.Fake<IHttpRequestHandler<RequestStub, ResponseStub>>();
        }

        public void Setup(IEnumerable<HateoasLink<RequestStub, ResponseStub>> links = null)
        {
            this.links = links ?? new List<HateoasLink<RequestStub, ResponseStub>>();
            sut = new WeblinkingHeaderRequestDecorator<RequestStub, ResponseStub>(inner, this.links);
        }

        public class Constructor : WeblinkingHeaderRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    A.Dummy<IHttpRequestHandler<RequestStub, ResponseStub>>(),
                    null,
                },
                new object[] { null, A.CollectionOfDummy<HateoasLink<RequestStub, ResponseStub>>(0) },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IHttpRequestHandler<RequestStub, ResponseStub> i,
                IEnumerable<HateoasLink<RequestStub, ResponseStub>> d)
            {
                /* Arrange */
                void shouldThrow() =>
                    new WeblinkingHeaderRequestDecorator<RequestStub, ResponseStub>(i, d);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : WeblinkingHeaderRequestDecoratorTests
        {
            private readonly CancellationToken cancelToken;
            private readonly HttpContext context;

            public HandleAsync()
            {
                context = A.Fake<HttpContext>();
                cancelToken = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task ResultIsError_DoesNothing()
            {
                /* Arrange */
                Setup();
                var error = Result.Error<HandlerResponse>(new Exception());
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Returns(error);

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal(error, result);
            }

            [Fact]
            public async Task NoLinks_NoHeadersAdded()
            {
                /* Arrange */
                Setup();
                var response = HandlerContext.Create(A.Dummy<RequestStub>(), A.Dummy<ResponseStub>());
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Returns(Result.Ok(response));

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => context.Response.Headers.Add(A<string>._, A<StringValues>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task HasLinks_HeadersAdded()
            {
                /* Arrange */
                var response = HandlerContext.Create(A.Dummy<RequestStub>(), A.Dummy<ResponseStub>());
                Setup(new HateoasLink<RequestStub, ResponseStub>[]
                    {
                        new ResponseHateoasLink(),
                        new ResponseHateoasLink2() { Title = "bar" }
                    });
                StringValues _;
                StringValues headerValue = default;
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Returns(Result.Ok(response));
                A.CallTo(() => context.Response.Headers.TryGetValue("Link", out _))
                    .Returns(false);
                A.CallTo(() => context.Response.Headers.Add("Link", A<StringValues>._))
                    .Invokes(c => headerValue = c.GetArgument<StringValues>(1));

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal(
                    new StringValues(new[]
                    {
                        "<https://foobar/lorem>;rel=foo",
                        "<https://foobar/ipsum>;rel=foo;title=bar"
                    }),
                    headerValue);
            }

            public class ResponseHateoasLink : HateoasLink<RequestStub, ResponseStub>
            {
                public ResponseHateoasLink() : base((t, r) => "lorem", "foo")
                { }
            }

            public class ResponseHateoasLink2 : HateoasLink<RequestStub, ResponseStub>
            {
                public ResponseHateoasLink2() : base((t, r) => "ipsum", "foo")
                { }
            }
        }
    }
}
