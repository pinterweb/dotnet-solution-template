namespace ShelLife.Domain.UnitTest
{
    using Xunit;
    using BusinessApp.Domain;

    public class ResultExtensionsTests
    {
        public class IgnoreValue : ResultExtensionsTests
        {
            [Fact]
            public void KeepsError()
            {
                /* Arrange */
                var error = 1;
                var sut = Result<string, int>.Error(error);

                /* Act */
                var newResult = sut.IgnoreValue();

                /* Assert */
                Assert.Equal(1, newResult.UnwrapError());
            }
        }
    }
}
