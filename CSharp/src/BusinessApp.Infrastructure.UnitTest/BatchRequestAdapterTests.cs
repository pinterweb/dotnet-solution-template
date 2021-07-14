using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class BatchRequestAdapterTests
    {
        private readonly CancellationToken cancelToken;
        private readonly BatchRequestAdapter<CommandStub, CommandStub> sut;
        private readonly IRequestHandler<CommandStub, CommandStub> inner;

        public BatchRequestAdapterTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<IRequestHandler<CommandStub, CommandStub>>();

            sut = new BatchRequestAdapter<CommandStub, CommandStub>(inner);
        }

        public class Constructor : BatchRequestAdapterTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null  },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IRequestHandler<CommandStub, CommandStub> c)
            {
                /* Arrange */
                void shouldThrow() => new BatchRequestAdapter<CommandStub, CommandStub>(c);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class HandleAsync : BatchRequestAdapterTests
        {
            [Fact]
            public async Task WithMultipleCommands_HandlerCalledForEach()
            {
                /* Arrange */
                var commands = new[] { A.Dummy<CommandStub>(), A.Dummy<CommandStub>() };

                /* Act */
                await sut.HandleAsync(commands, cancelToken);

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands.First(), cancelToken))
                    .MustHaveHappenedOnceExactly();
                A.CallTo(() => inner.HandleAsync(commands.Last(), cancelToken))
                    .MustHaveHappenedOnceExactly();
            }

            public class OnError : BatchRequestAdapterTests
            {
                private readonly Exception errorException;
                private readonly Result<CommandStub, Exception> error;
                private readonly Result<CommandStub, Exception> ok;
                private readonly IEnumerable<CommandStub> commands;

                public OnError()
                {
                    commands = new[]
                    {
                        new CommandStub(),
                        new CommandStub(),
                        new CommandStub(),
                    };

                    errorException = new Exception();
                    error = Result.Error<CommandStub>(errorException);
                    ok = Result.Ok(A.Dummy<CommandStub>());

                    A.CallTo(() => inner.HandleAsync(A<CommandStub>._, cancelToken))
                        .Returns(error).Once()
                        .Then.Returns(Result.Ok(commands.ElementAt(1))).Once()
                        .Then.Returns(Result.Ok(commands.ElementAt(2))).Once();
                }

                [Fact]
                public async Task AllReturnedInBatchException()
                {
                    /* Act */
                    var results = await sut.HandleAsync(commands, cancelToken);

                    /* Assert */
                    Assert.IsType<BatchException>(results.UnwrapError());
                }

                [Fact]
                public async Task AllReturnedInBatchExceptionInOrder()
                {
                    /* Act */
                    var results = await sut.HandleAsync(commands, cancelToken);

                    /* Assert */
                    var ex = Assert.IsType<BatchException>(results.UnwrapError());
                    Assert.Collection(ex,
                        r => Assert.Equal(Result.Error<object>(errorException), r),
                        r => Assert.Equal(Result.Ok<object>(commands.ElementAt(1)), r),
                        r => Assert.Equal(Result.Ok<object>(commands.ElementAt(2)), r)
                    );
                }
            }

            [Fact]
            public async Task AllOkResults_ResultsReturned()
            {
                /* Arrange */
                var commands = new[]
                {
                    new CommandStub(),
                    new CommandStub(),
                };
                var result1 = new CommandStub();
                var result2 = new CommandStub();

                var ok1 = Result.Ok(result1);
                var ok2 = Result.Ok(result2);

                A.CallTo(() => inner.HandleAsync(A<CommandStub>._, cancelToken))
                    .Returns(ok1).Once().Then.Returns(ok2);

                /* Act */
                var results = await sut.HandleAsync(commands, cancelToken);

                /* Assert */
                Assert.Collection(results.Unwrap(),
                    v => Assert.Same(result1, v),
                    v => Assert.Same(result2, v)
                );
            }
        }
    }
}
