namespace BusinessApp.Domain.UnitTest
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using BusinessApp.Test.Shared;
    using Xunit;
    using FakeItEasy;

    public class DomainEventHandlerTests
    {
        private readonly IEventRepository repo;
        private readonly DomainEventHandler<DomainEventStub> sut;

        public DomainEventHandlerTests()
        {
            repo = A.Fake<IEventRepository>();
            sut = new DomainEventHandler<DomainEventStub>(repo);
        }

        public class Constructor : DomainEventHandlerTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[] { null  }
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(IEventRepository r)
            {
                /* Arrange */
                void shouldThrow() => new DomainEventHandler<DomainEventStub>(r);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }
        }

        public class HandleAsync : DomainEventHandlerTests
        {
            [Fact]
            public async Task AddsToRepo()
            {
                /* Arrange */
                var e = new DomainEventStub();

                /* Act */
                var result = await sut.HandleAsync(e, A.Dummy<CancellationToken>());

                /* Assert */
                A.CallTo(() => repo.Add(e)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ReturnsOkResult()
            {
                /* Arrange */
                var e = new DomainEventStub();

                /* Act */
                var result = await sut.HandleAsync(e, A.Dummy<CancellationToken>());

                /* Assert */
                Assert.Empty(result.Unwrap());
            }
        }
    }
}
