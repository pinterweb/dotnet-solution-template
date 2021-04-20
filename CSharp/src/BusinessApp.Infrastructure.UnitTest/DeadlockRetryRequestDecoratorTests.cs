using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;
using System.Threading;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class DeadlockRetryRequestDecoratorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly DeadlockRetryRequestDecorator<CommandStub, CommandStub> sut;
        private readonly IRequestHandler<CommandStub, CommandStub> inner;

        public DeadlockRetryRequestDecoratorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<CommandStub, CommandStub>>();

            sut = new DeadlockRetryRequestDecorator<CommandStub, CommandStub>(inner);
        }

        public class Constructor : DeadlockRetryRequestDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IRequestHandler<CommandStub, CommandStub> c)
            {
                /* Arrange */
                void shouldThrow() => new DeadlockRetryRequestDecorator<CommandStub, CommandStub>(c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : DeadlockRetryRequestDecoratorTests
        {
            [Fact]
            public async Task NormalException_DoesNotHandle()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, cancelToken));

                /* Assert */
                Assert.NotNull(ex);
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task DbExceptionNotADeadlock_DoesNotHandle()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                var exception = A.Fake<DbException>();
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).Throws(exception);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, cancelToken));

                /* Assert */
                Assert.NotNull(ex);
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task DbExceptionIsDeadlock_Retries5times()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                var exception = A.Fake<DbException>();
                A.CallTo(() => exception.Message).Returns("foobar deadlock lorem");
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).Throws(exception).NumberOfTimes(4);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, cancelToken));

                /* Assert */
                Assert.Null(ex);
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).MustHaveHappened(5, Times.Exactly);
            }

            [Fact]
            public async Task DbExceptionIsDeadlockAfterFiveTimes_ReturnsErrorResultCommunicationException()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                var exception = A.Fake<DbException>();
                A.CallTo(() => exception.Message).Returns("foobar deadlock lorem");
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).Throws(exception).NumberOfTimes(5);

                /* Act */
                var ex = await sut.HandleAsync(command, cancelToken);

                /* Assert */
                var error = Assert.IsType<CommunicationException>(ex.UnwrapError());
                Assert.Equal(
                    "There was a conflict saving your data. Please retry your " +
                    "operation again. If you continue to see this message, please " +
                    "contact support.",
                    error.Message);
            }

            [Fact]
            public async Task DbExceptionInInnerExceptionIsDeadlock_Retries5times()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                var dbException = A.Fake<DbException>();
                A.CallTo(() => dbException.Message).Returns("foobar deadlock lorem");
                var exception = new Exception("foo", new Exception("bar", dbException));
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).Throws(exception).NumberOfTimes(4);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, cancelToken));

                /* Assert */
                Assert.Null(ex);
                A.CallTo(() => inner.HandleAsync(command, cancelToken)).MustHaveHappened(5, Times.Exactly);
            }
        }
    }
}
