namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading.Tasks;
    using FakeItEasy;
    using BusinessApp.App;
    using Xunit;
    using System.Threading;
    using BusinessApp.Domain;

    public class DeadlockRetryDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly DeadlockRetryDecorator<CommandStub> sut;
        private readonly ICommandHandler<CommandStub> inner;

        public DeadlockRetryDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<CommandStub>>();

            sut = new DeadlockRetryDecorator<CommandStub>(inner);
        }

        public class Constructor : DeadlockRetryDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(ICommandHandler<CommandStub> c)
            {
                /* Arrange */
                void shouldThrow() => new DeadlockRetryDecorator<CommandStub>(c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : DeadlockRetryDecoratorTests
        {
            [Fact]
            public async Task NormalException_DoesNotHandle()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                A.CallTo(() => inner.HandleAsync(command, token)).Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, token));

                /* Assert */
                Assert.NotNull(ex);
                A.CallTo(() => inner.HandleAsync(command, token)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task DbExceptionNotADeadlock_DoesNotHandle()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                var exception = A.Fake<DbException>();
                A.CallTo(() => inner.HandleAsync(command, token)).Throws(exception);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, token));

                /* Assert */
                Assert.NotNull(ex);
                A.CallTo(() => inner.HandleAsync(command, token)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task DbExceptionIsDeadlock_Retries5times()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                var exception = A.Fake<DbException>();
                A.CallTo(() => exception.Message).Returns("foobar deadlock lorem");
                A.CallTo(() => inner.HandleAsync(command, token)).Throws(exception).NumberOfTimes(4);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, token));

                /* Assert */
                Assert.Null(ex);
                A.CallTo(() => inner.HandleAsync(command, token)).MustHaveHappened(5, Times.Exactly);
            }

            [Fact]
            public async Task DbExceptionIsDeadlockAfterFiveTimes_ReturnsErrorResultCommunicationException()
            {
                /* Arrange */
                var command = A.Dummy<CommandStub>();
                var exception = A.Fake<DbException>();
                A.CallTo(() => exception.Message).Returns("foobar deadlock lorem");
                A.CallTo(() => inner.HandleAsync(command, token)).Throws(exception).NumberOfTimes(5);

                /* Act */
                var ex = await sut.HandleAsync(command, token);

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
                A.CallTo(() => inner.HandleAsync(command, token)).Throws(exception).NumberOfTimes(4);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, token));

                /* Assert */
                Assert.Null(ex);
                A.CallTo(() => inner.HandleAsync(command, token)).MustHaveHappened(5, Times.Exactly);
            }
        }
    }
}
