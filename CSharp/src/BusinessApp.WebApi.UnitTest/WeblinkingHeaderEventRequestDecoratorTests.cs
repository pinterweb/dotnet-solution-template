using FakeItEasy;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using BusinessApp.Test.Shared;

namespace BusinessApp.WebApi.UnitTest
{
    using HandlerResponse = HandlerContext<RequestStub, CompoisteEventStub>;

    public class WeblinkingHeaderEventRequestDecoratorTests
    {
        private readonly IHttpRequestHandler<RequestStub, CompoisteEventStub> inner;
        private IDictionary<Type, HateoasLink<RequestStub, IDomainEvent>> links;
        private WeblinkingHeaderEventRequestDecorator<RequestStub, CompoisteEventStub> sut;

        public WeblinkingHeaderEventRequestDecoratorTests()
        {
            inner = A.Fake<IHttpRequestHandler<RequestStub, CompoisteEventStub>>();
        }

        public void Setup(IDictionary<Type, HateoasLink<RequestStub, IDomainEvent>> links = null)
        {
            this.links = links ?? new Dictionary<Type, HateoasLink<RequestStub, IDomainEvent>>();
            sut = new WeblinkingHeaderEventRequestDecorator<RequestStub, CompoisteEventStub>(inner, this.links);
        }

        public class Constructor : WeblinkingHeaderEventRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    A.Dummy<IHttpRequestHandler<RequestStub, CompoisteEventStub>>(),
                    null,
                },
                new object[] { null, new Dictionary<Type, HateoasLink<RequestStub, IDomainEvent>>() },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IHttpRequestHandler<RequestStub, CompoisteEventStub> i,
                IDictionary<Type, HateoasLink<RequestStub, IDomainEvent>> d)
            {
                /* Arrange */
                void shouldThrow() =>
                    new WeblinkingHeaderEventRequestDecorator<RequestStub, CompoisteEventStub>(i, d);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : WeblinkingHeaderEventRequestDecoratorTests
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
            public async Task NoEventLinks_NoHeadersAdded()
            {
                /* Arrange */
                Setup();
                var events = new CompoisteEventStub
                {
                    Events = A.CollectionOfDummy<IDomainEvent>(2)
                };
                var innerResult = Result.Ok(HandlerContext.Create(A.Dummy<RequestStub>(), events));
                A.CallTo(() => inner.HandleAsync(context, cancelToken))
                    .Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                A.CallTo(() => context.Response.Headers.Add(A<string>._, A<StringValues>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task HasEventLink_HeaderAdded()
            {
                /* Arrange */
                Setup(new Dictionary<Type, HateoasLink<RequestStub, IDomainEvent>>
                {
                    { typeof(EventStub) ,new EventStubEventHateoasLink() { Title = "bar" } }
                });
                var events = new CompoisteEventStub
                {
                    Events = new[] { new EventStub(), new EventStub() { Id = 2 } }
                };
                var innerResult = Result.Ok(HandlerContext.Create(A.Dummy<RequestStub>(), events));
                StringValues _;
                StringValues headerValue = default;
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Returns(innerResult);
                A.CallTo(() => context.Response.Headers.TryGetValue("Link", out _))
                    .Returns(false);
                A.CallTo(() => context.Response.Headers.Add("Link", A<StringValues>._))
                    .Invokes(c => headerValue = c.GetArgument<StringValues>(1));

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal(
                    new StringValues(new []
                    {
                        "<https://foobar/id/0>;rel=foo;title=bar",
                        "<https://foobar/id/2>;rel=foo;title=bar"
                    }),
                    headerValue);
            }

            [Fact]
            public async Task HasEventLinksWithExistingLinkHeader_HeadersAdded()
            {
                /* Arrange */
                Setup(new Dictionary<Type, HateoasLink<RequestStub, IDomainEvent>>
                {
                    { typeof(EventStub) ,new EventStubEventHateoasLink() }
                });
                StringValues initialHeader = new StringValues("lorem");
                StringValues headerValue = default;
                var events = new CompoisteEventStub
                {
                    Events = new[] { new EventStub() }
                };
                var innerResult = Result.Ok(HandlerContext.Create(A.Dummy<RequestStub>(), events));
                A.CallTo(() => inner.HandleAsync(context, cancelToken)).Returns(innerResult);
                A.CallTo(() => context.Response.Headers.TryGetValue("Link", out initialHeader))
                    .Returns(true);
                A.CallToSet(() => context.Response.Headers["Link"])
                    .Invokes(c => headerValue = c.GetArgument<StringValues>(1));

                /* Act */
                var result = await sut.HandleAsync(context, cancelToken);

                /* Assert */
                Assert.Equal(
                    new StringValues(new[] { "lorem", "<https://foobar/id/0>;rel=foo" }),
                    headerValue);
            }

            public class EventStub : IDomainEvent
            {
                public int Id { get; set; }
                public DateTimeOffset OccurredUtc { get; }
            }

            public class EventStubEventHateoasLink : HateoasEventLink<RequestStub, EventStub>
            {
                public EventStubEventHateoasLink() : base("foo")
                { }

                protected override Func<RequestStub, EventStub, string> EventRelativeLinkFactory
                    => (r, e) => $"id/{e.Id.ToString()}";
            }
        }
    }
}
