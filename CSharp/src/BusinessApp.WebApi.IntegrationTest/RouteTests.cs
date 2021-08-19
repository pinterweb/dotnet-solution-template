using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SimpleInjector;
using Xunit;
using Xunit.Abstractions;

namespace BusinessApp.WebApi.IntegrationTest
{
    using BusinessApp.WebApi;

    public class RouteTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;
        private readonly ITestOutputHelper output;

        public RouteTests(WebApplicationFactory<Startup> factory,
            ITestOutputHelper output)
        {
            this.factory = factory;
            this.output = output;
        }

        [Fact]
        public async Task GivenARequestToGetOneResource_WhenIsValidQuery_ReturnsOneResult()
        {
            // Given
            var client = factory.NewClient();

            // When
            var response = await client.GetAsync("/api/resources/1");

            // Then
            var resource = await response.Success<Response>(output);
            Assert.Equal(1, resource.Id);
        }

        [Fact]
        public async Task GivenARequestToGetManyResources_WhenIsAnEnvelope_HeadersReturnedToo()
        {
            // Given
            var client = factory.NewClient();

            // When
            var response = await client.GetAsync("/api/resources");

            // Then
            await response.Success(output);
            var accessHeader = Assert.Single(
                response.Headers.GetValues("Access-Control-Expose-Headers"));
#if DEBUG
            var pageHeader = Assert.Single(
                response.Headers.GetValues("VND.foobar.pagination"));
            Assert.Equal("VND.foobar.pagination", accessHeader);
            Assert.Equal("count=1", pageHeader);
#else
            var pageHeader = Assert.Single(
                response.Headers.GetValues("VND.$(lower_appname).pagination"));
            Assert.Equal("VND.$(lower_appname).pagination", accessHeader);
            Assert.Equal("count=1", pageHeader);
#endif
        }

        [Fact]
        public async Task GivenARequestToSaveANewResource_WhenIsValidRequest_EchosResut()
        {
            // Given
            var client = factory.NewClient();

            var payload = new { id = 1, longerId = 99 };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await client.PostAsync("/api/resources", content);

            // Then
            await response.Success(output);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(
                "{  \"longerId\": \"99\",  \"id\": 1}",
                json.Replace(Environment.NewLine, ""));
        }

        [Fact]
        public async Task GivenARequestToReplaceAResource_WhenIsSuccessful_NoContentReturned()
        {
            // Given
            var client = factory.NewClient();

            var payload = new { id = 1 };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await client.PutAsync("/api/resources/1", content);

            // Then
            await response.Success(output);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GivenARequestToDeleteAResource_WhenIsSuccessful_NoContentReturned()
        {
            // Given
            var notifier = A.Fake<ITestNotifier>();
            Container testContainer = null;
            var client = factory.NewClient(container =>
            {
                container.RegisterInstance(notifier);
#if DEBUG
                container.RegisterDecorator(
                    typeof(IEventHandler<Delete.WebDomainEvent>),
                    typeof(EventDecorator));

                container.RegisterInstance(GetEventLinks<Delete.Query>());
#elif (events && usehateoas)
                container.RegisterDecorator(
                    typeof(IEventHandler<Delete.WebDomainEvent>),
                    typeof(EventDecorator));
                container.RegisterInstance(GetEventLinks<Delete.Query>());
#endif

                testContainer = container;
            });
            var payload = new { id = 1 };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");
#if DEBUG
            var expectedCalls = testContainer
                .GetAllInstances<IEventHandler<Delete.WebDomainEvent>>()
                .Count() * 2;
#elif events
            // each handler will fire twice since events are chained
            var expectedCalls = testContainer
                .GetAllInstances<IEventHandler<Delete.WebDomainEvent>>()
                .Count() * 2;
#endif

            // When
            var response = await client.DeleteAsync("/api/resources/1");

            // Then
            await response.Success(output);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
#if DEBUG
            A.CallTo(() => notifier.Notify())
                .MustHaveHappened(expectedCalls, Times.Exactly);
#elif events
            A.CallTo(() => notifier.Notify())
                .MustHaveHappened(expectedCalls, Times.Exactly);
#endif
        }

#if DEBUG
        /// <summary>
        /// Test that are events are firing correctly;
        /// </summary>
        public class EventDecorator : IEventHandler<Delete.WebDomainEvent>
        {
            private readonly IEventHandler<Delete.WebDomainEvent> inner;
            private readonly ITestNotifier tester;

            public EventDecorator(ITestNotifier tester, IEventHandler<Delete.WebDomainEvent> inner)
            {
                this.inner = inner;
                this.tester = tester;
            }

            public Task<Result<IEnumerable<IDomainEvent>, System.Exception>> HandleAsync(
                Delete.WebDomainEvent e, CancellationToken cancelToken)
            {
                tester.Notify();
                return inner.HandleAsync(e, cancelToken);
            }
        }
#elif events
        /// <summary>
        /// Test that are events are firing correctly;
        /// </summary>
        public class EventDecorator : IEventHandler<Delete.WebDomainEvent>
        {
            private readonly IEventHandler<Delete.WebDomainEvent> inner;
            private readonly ITestNotifier tester;

            public EventDecorator(ITestNotifier tester, IEventHandler<Delete.WebDomainEvent> inner)
            {
                this.inner = inner;
                this.tester = tester;
            }

            public Task<Result<IEnumerable<IDomainEvent>, System.Exception>> HandleAsync(
                Delete.WebDomainEvent e, CancellationToken cancelToken)
            {
                tester.Notify();
                return inner.HandleAsync(e, cancelToken);
            }
        }
#endif

        /// <summary>
        /// Service to inject to test actual services are called
        /// </summary>
        public interface ITestNotifier
        {
            void Notify();
        }

        public class Response
        {
            public int Id { get; set; }
        }

#if DEBUG
        private IDictionary<Type, HateoasLink<T, IDomainEvent>> GetEventLinks<T>()
            => new Dictionary<Type, HateoasLink<T, IDomainEvent>>();
#elif (usehateoas && events)
        private IDictionary<Type, HateoasLink<T, IDomainEvent>> GetEventLinks<T>()
            => new Dictionary<Type, HateoasLink<T, IDomainEvent>>();
#endif
    }
}
