using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class AutomationRequestDecoratorTests
    {
        private readonly AutomationRequestDecorator<QueryStub, CompositeEventStub> sut;
        private readonly IRequestHandler<QueryStub, CompositeEventStub> inner;
        private readonly IProcessManager manager;

        public AutomationRequestDecoratorTests()
        {
            inner = A.Fake<IRequestHandler<QueryStub, CompositeEventStub>>();
            manager = A.Fake<IProcessManager>();

            sut = new AutomationRequestDecorator<QueryStub, CompositeEventStub>(inner, manager);
        }

        public class Constructor : AutomationRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[]
                        {
                            null,
                            A.Dummy<IProcessManager>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<QueryStub, CompositeEventStub>>(),
                            null,
                        },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void WithAnyNullService_ExceptionThrown(
                IRequestHandler<QueryStub, CompositeEventStub> d, IProcessManager p)
            {
                /* Arrange */
                Action create = () => new AutomationRequestDecorator<QueryStub, CompositeEventStub>(d, p);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BusinessAppException>(exception);
            }
        }

        public class HandleAsync : AutomationRequestDecoratorTests
        {
            private readonly QueryStub query;
            private readonly CancellationToken cancelToken;

            public HandleAsync()
            {
                query = new QueryStub();
                cancelToken = A.Dummy<CancellationToken>();
            }

            [Fact]
            public async Task InnerResultHasError_ManagerNotCalled()
            {
                /* Arrange */
                var result = Result.Error<CompositeEventStub>(new Exception());
                A.CallTo(() => inner.HandleAsync(query, cancelToken)).Returns(result);

                /* Act */
                var _ = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                A.CallTo(() => manager.HandleNextAsync(A<IEnumerable<IEvent>>._, A<CancellationToken>._))
                    .MustNotHaveHappened();
            }

            [Fact]
            public async Task InnerResultHasError_Returned()
            {
                /* Arrange */
                var result = Result.Error<CompositeEventStub>(new Exception());
                A.CallTo(() => inner.HandleAsync(query, cancelToken)).Returns(result);

                /* Act */
                var handlerResult = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(result, handlerResult);
            }

            [Fact]
            public async Task InnerResultIsOk_Returned()
            {
                /* Arrange */
                var result = Result.Ok<CompositeEventStub>(new CompositeEventStub());
                A.CallTo(() => inner.HandleAsync(query, cancelToken)).Returns(result);

                /* Act */
                var handlerResult = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Equal(result, handlerResult);
            }

            [Fact]
            public async Task InnerResultIsOk_ManagerErrorReturned()
            {
                /* Arrange */
                var managerResult = Result.Error(new Exception());
                var @event = A.Dummy<CompositeEventStub>();
                var result = Result.Ok<CompositeEventStub>(@event);
                A.CallTo(() => inner.HandleAsync(query, cancelToken)).Returns(result);
                A.CallTo(() => manager.HandleNextAsync(@event.Events, cancelToken))
                    .Returns(managerResult);

                /* Act */
                var handlerResult = await sut.HandleAsync(query, cancelToken);

                /* Assert */
                Assert.Same(managerResult.UnwrapError(), handlerResult.UnwrapError());
            }
        }
    }
}
