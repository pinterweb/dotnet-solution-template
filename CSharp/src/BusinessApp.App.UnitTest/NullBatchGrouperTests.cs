namespace BusinessApp.App.UnitTest
{
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class NullBatchGrouperTests
    {
        private readonly CancellationToken token;
        private readonly NullBatchGrouper<CommandStub> sut;

        public NullBatchGrouperTests()
        {
            token = A.Dummy<CancellationToken>();

            sut = new NullBatchGrouper<CommandStub>();
        }

        public class GroupAsync : NullBatchGrouperTests
        {
            [Fact]
            public async Task DefaultBehavior_OneGroupReturned()
            {
                /* Arrange */
                var commands = new[] { new CommandStub(), new CommandStub() };

                /* Act */
                var grouping = await sut.GroupAsync(commands, token);

                /* Assert */
                var onlygroup = Assert.Single(grouping);
                Assert.Same(commands, onlygroup);
            }
        }
    }
}
