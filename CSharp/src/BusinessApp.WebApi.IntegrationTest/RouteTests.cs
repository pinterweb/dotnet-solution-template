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
using BusinessApp.Infrastructure;
using BusinessApp.WebApi;

namespace BusinessApp.WebApi.IntegrationTest
{
    [Collection(nameof(DatabaseCollectionFixture))]
    public class RouteTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;
        private readonly ITestOutputHelper output;
        private readonly WebApiDatabaseFixture db;

        public RouteTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output,
            WebApiDatabaseFixture db)
        {
            this.factory = factory;
            this.output = output;
            this.db = db;
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

            var payload = new { id = 1, longerId = 1 };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await client.PostAsync("/api/resources", content);

            // Then
            await response.Success(output);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(
                "{\"longerId\":\"1\",\"id\":1}",
                json.Replace(Environment.NewLine, "").Replace(" ", ""));
        }

        [Fact]
        public async Task GivenARequestToSaveManyResources_WhenIsValidRequest_EchosResut()
        {
            // Given
            var grouper = A.Fake<IBatchGrouper<PostOrPut.Body>>();
            A.CallTo(() => grouper.GroupAsync(A<IEnumerable<PostOrPut.Body>>._, A<CancellationToken>._))
                .Returns(new[]
                {
                    new[] { new PostOrPut.Body { Id = new EntityId(2), LongerId = 99 } },
                    new[] { new PostOrPut.Body { Id = new EntityId(3), LongerId = 97 } },
                    new[] { new PostOrPut.Body { Id = new EntityId(4), LongerId = 96 } },
                    new[] { new PostOrPut.Body { Id = new EntityId(5), LongerId = 95 } },
                    new[] { new PostOrPut.Body { Id = new EntityId(6), LongerId = 94 } },
                });
            var client = factory.NewClient((c, s) =>
            {
                c.RegisterInstance<IBatchGrouper<PostOrPut.Body>>(grouper);
            });

            var payload = new[]
            {
                new { id = 1, longerId = 99 },
                new { id = 2, longerId = 97 },
                new { id = 3, longerId = 96 },
                new { id = 4, longerId = 95 },
                new { id = 5, longerId = 94 },
            };
            var content = new StringContent(JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            // When
            var response = await client.PostAsync("/api/resources", content);

            // Then
            await response.Success(output);
            var json = await response.Content.ReadAsStringAsync();
            var savedData = db.DbContext.Set<PostOrPut.Body>()
                .Where(p => p.LongerId >= 94 && p.LongerId <= 99)
                .ToList().OrderBy(p => p.LongerId);
            Assert.Collection(savedData,
                p => Assert.Equal(94, p.LongerId),
                p => Assert.Equal(95, p.LongerId),
                p => Assert.Equal(96, p.LongerId),
                p => Assert.Equal(97, p.LongerId),
                p => Assert.Equal(99, p.LongerId));
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
            var client = factory.NewClient((container, services) =>
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

            public Task<Result<IEnumerable<IEvent>, System.Exception>> HandleAsync(
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

            public Task<Result<IEnumerable<IEvent>, System.Exception>> HandleAsync(
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
        private IDictionary<Type, HateoasLink<T, IEvent>> GetEventLinks<T>()
            => new Dictionary<Type, HateoasLink<T, IEvent>>();
#elif (usehateoas && events)
        private IDictionary<Type, HateoasLink<T, IEvent>> GetEventLinks<T>()
            => new Dictionary<Type, HateoasLink<T, IEvent>>();
#endif
    }
}
