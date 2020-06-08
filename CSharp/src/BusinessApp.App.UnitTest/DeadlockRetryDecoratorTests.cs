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

    public class DeadlockRetryDecoratorTests
    {
        private readonly CancellationToken token;
        private readonly DeadlockRetryDecorator<DummyCommand> sut;
        private readonly ICommandHandler<DummyCommand> inner;

        public DeadlockRetryDecoratorTests()
        {
            token = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<DummyCommand>>();

            sut = new DeadlockRetryDecorator<DummyCommand>(inner);
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
            public void InvalidCtorArgs_ExceptionThrown(ICommandHandler<DummyCommand> c)
            {
                /* Arrange */
                void shouldThrow() => new DeadlockRetryDecorator<DummyCommand>(c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<ArgumentNullException>(ex);

            }
        }

        public class HandleAsync : DeadlockRetryDecoratorTests
        {
            [Fact]
            public async Task NormalException_DoesNothing()
            {
                /* Arrange */
                var command = A.Dummy<DummyCommand>();
                A.CallTo(() => inner.HandleAsync(command, token)).Throws<Exception>();

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, token));

                /* Assert */
                A.CallTo(() => inner.HandleAsync(command, token)).MustHaveHappened();
            }

            [Fact]
            public async Task DbExceptionNotADeadlock_DoesNothing()
            {
                /* Arrange */
                var command = A.Dummy<DummyCommand>();
                var exception = A.Fake<DbException>();
                A.CallTo(() => inner.HandleAsync(command, token)).Throws(exception);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, token));

                /* Assert */
                A.CallTo(() => inner.HandleAsync(command, token)).MustHaveHappened();
            }

            [Fact]
            public async Task DbExceptionIsDeadlock_Retries5times()
            {
                /* Arrange */
                var command = A.Dummy<DummyCommand>();
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
            public async Task DbExceptionIsDeadlockAfterFiveTimes_ThrowsCommunicationException()
            {
                /* Arrange */
                var command = A.Dummy<DummyCommand>();
                var exception = A.Fake<DbException>();
                A.CallTo(() => exception.Message).Returns("foobar deadlock lorem");
                A.CallTo(() => inner.HandleAsync(command, token)).Throws(exception).NumberOfTimes(5);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(command, token));

                /* Assert */
                Assert.IsType<CommunicationException>(ex);
                Assert.Equal(
                    "There was a conflict saving your data. Please retry your " +
                    "operation again. If you continue to see this message, please " +
                    "contact support.",
                    ex.Message);
            }

            [Fact]
            public async Task DbExceptionInInnerExceptionIsDeadlock_Retries5times()
            {
                /* Arrange */
                var command = A.Dummy<DummyCommand>();
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
