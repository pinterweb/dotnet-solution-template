namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class BackgroundWorkerTests : IDisposable
    {
        private readonly TestWorker sut;

        public BackgroundWorkerTests()
        {
            sut = A.Fake<TestWorker>(opt => opt.Wrapping(new TestWorker()));
        }

        [Fact]
        public async Task Enqueue_WhenHasEntry_ConsumeCalledOnce()
        {
            /* Arrange */
            var entry = "bar";

            /* Act */
            sut.Run(entry);
            await Task.Delay(TimeSpan.FromSeconds(3));

            /* Assert */
            A.CallTo(sut).Where(x => x.Method.Name == "Consume")
                .WhenArgumentsMatch(args => args.Get<string>(0) == "bar")
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Enqueue_WhenHasMultipleEntry_RemovesThemFromQueueInOrder()
        {
            /* Arrange */
            sut.Run("foo");

            /* Act */
            sut.Run("bar");
            await Task.Delay(TimeSpan.FromSeconds(3));

            /* Assert */
            A.CallTo(sut).Where(x => x.Method.Name == "Consume")
                .WhenArgumentsMatch(args => args.Get<string>(0) == "foo")
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(sut).Where(x => x.Method.Name == "Consume")
                        .WhenArgumentsMatch(args => args.Get<string>(0) == "bar")
                        .MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task Enqueue_WhenDisposed_OnAddFailedCalled()
        {
            /* Arrange */
            sut.Dispose();

            /* Act */
            sut.Run("bar");
            await Task.Delay(TimeSpan.FromSeconds(3));

            /* Assert */
            A.CallTo(sut).Where(x => x.Method.Name == "Consume")
                .MustNotHaveHappened();
            A.CallTo(sut).Where(x => x.Method.Name == "OnAddFailed")
                .WhenArgumentsMatch(args => args.Get<string>(0) == "bar")
                .MustHaveHappenedOnceExactly();
        }

        public void Dispose() => sut.Dispose();

        public class TestWorker : BackgroundWorker<string>
        {
            public void Run(string foo) => EnQueue(foo);

            protected override void Consume(string t) { }

            protected override void EnqueueFailed(Exception ex) { }

            protected override void OnAddFailed(string t) { }
        }
    }
}
