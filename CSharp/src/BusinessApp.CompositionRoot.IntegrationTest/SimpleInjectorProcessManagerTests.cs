using Xunit;
using SimpleInjector;
using FakeItEasy;
using BusinessApp.Infrastructure;
using BusinessApp.Test.Shared;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using BusinessApp.Domain;

namespace BusinessApp.CompositionRoot.IntegrationTest
{
    public class SimpleInjectorProcessManagerTests : IDisposable
    {
        private readonly Container container;
        private readonly IRequestStore store;
        private readonly SimpleInjectorProcessManager sut;
        private readonly IRequestHandler<RequestStub, ResponseStub> handler;
        private readonly IRequestMapper<RequestStub, DomainEventStub> mapper;

        public SimpleInjectorProcessManagerTests()
        {
            container = new Container();
            store = A.Fake<IRequestStore>();
            handler = A.Fake<IRequestHandler<RequestStub, ResponseStub>>();
            mapper = A.Fake<IRequestMapper<RequestStub, DomainEventStub>>();

            sut = new SimpleInjectorProcessManager(container, store);

            container.RegisterInstance(handler);
            container.RegisterInstance(mapper);
        }

        public void Dispose() => container.Dispose();

        public class Constructor : SimpleInjectorProcessManagerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<IRequestStore>() }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(Container c, IRequestStore s)
            {
                /* Arrange */
                void shouldThrow() => new SimpleInjectorProcessManager(c, s);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleNextAsync : SimpleInjectorProcessManagerTests
        {
            private readonly CancellationToken cancelToken;

            public HandleNextAsync()
            {
                cancelToken = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task StoreReturnsNoRequestMetadata_HandlerNotCalled()
            {
                /* Arrange */
                A.CallTo(() => store.GetAllAsync()).Returns(A.CollectionOfDummy<RequestMetadata>(0));

                /* Act */
                await sut.HandleNextAsync(A.CollectionOfDummy<IDomainEvent>(1), cancelToken);

                /* Assert */
                A.CallTo(() => handler.HandleAsync(A<RequestStub>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task StoreReturnsNoRequestMetadata_MapperNotCalled()
            {
                /* Arrange */
                A.CallTo(() => store.GetAllAsync()).Returns(A.CollectionOfDummy<RequestMetadata>(0));

                /* Act */
                await sut.HandleNextAsync(A.CollectionOfDummy<IDomainEvent>(1), cancelToken);

                /* Assert */
                A.CallTo(() => mapper.Map(A<RequestStub>._, A<DomainEventStub>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task StoreReturnsRequestMetadata_WhenNoMatchedEvents_DoesNothing()
            {
                /* Arrange */
                var metadata = new RequestMetadata(typeof(RequestStub), typeof(ResponseStub))
                    .SetProp(nameof(RequestMetadata.EventTriggers), new[] { A.Dummy<Type>() });
                A.CallTo(() => store.GetAllAsync()).Returns(new[] { metadata });

                /* Act */
                await sut.HandleNextAsync(new[] { new DomainEventStub() }, cancelToken);

                /* Assert */
                A.CallTo(() => mapper.Map(A<RequestStub>._, A<DomainEventStub>._))
                    .MustNotHaveHappened();
                A.CallTo(() => handler.HandleAsync(A<RequestStub>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task StoreReturnsRequestMetadata_WithMatchedEvents_Handles()
            {
                /* Arrange */
                var @event = new DomainEventStub();
                RequestStub request = null;
                var metadata = new RequestMetadata(typeof(RequestStub), typeof(ResponseStub))
                    .SetProp(nameof(RequestMetadata.EventTriggers), new[] { typeof(DomainEventStub) });
                A.CallTo(() => store.GetAllAsync()).Returns(new[] { metadata });
                A.CallTo(() => mapper.Map(A<RequestStub>._, @event))
                    .Invokes(c => request = c.GetArgument<RequestStub>(0));

                /* Act */
                await sut.HandleNextAsync(new[] { @event }, cancelToken);

                /* Assert */
                A.CallTo(() => handler.HandleAsync(request, cancelToken))
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task RequestHandled_WithError_Returned()
            {
                /* Arrange */
                var exception = new Exception();
                var metadata = new RequestMetadata(typeof(RequestStub), typeof(ResponseStub))
                    .SetProp(nameof(RequestMetadata.EventTriggers), new[] { typeof(DomainEventStub) });
                A.CallTo(() => store.GetAllAsync()).Returns(new[] { metadata });
                A.CallTo(() => handler.HandleAsync(A<RequestStub>._, cancelToken))
                    .Returns(Result.Error<ResponseStub>(exception));

                /* Act */
                var result = await sut.HandleNextAsync(new[] { new DomainEventStub() }, cancelToken);

                /* Assert */
                Assert.Equal(Result.Error<Unit>(exception), result);
            }
        }
    }
}
