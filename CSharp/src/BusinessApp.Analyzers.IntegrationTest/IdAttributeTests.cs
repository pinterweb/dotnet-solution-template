using Xunit;

namespace BusinessApp.Analyzers.IntegrationTest
{
    public class IdAttributeTests
    {
        public class EqualsOverride : IdAttributeTests
        {
            [Theory]
            [InlineData("foo", true)]
            [InlineData("FOO", true)]
            [InlineData("bar", false)]
            public void WhenPropertiesEqual_TrueReturned(string b, bool expectTrue)
            {
                /* Arrange */
                var sut = new EntityStub { Id = "foo" };
                var other = new EntityStub { Id = b };

                /* Act */
                var areEqual = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectTrue, areEqual);
            }
        }
    }
}
