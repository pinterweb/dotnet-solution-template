using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class NullBatchGrouperTests
    {
        private readonly CancellationToken cancelToken;
        private readonly NullBatchGrouper<CommandStub> sut;

        public NullBatchGrouperTests()
        {
            cancelToken = A.Dummy<CancellationToken>();

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
                var grouping = await sut.GroupAsync(commands, cancelToken);

                /* Assert */
                var onlygroup = Assert.Single(grouping);
                Assert.Same(commands, onlygroup);
            }
        }
    }
}
