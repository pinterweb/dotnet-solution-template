namespace BusinessApp.Domain.UnitTest
{
    using Xunit;

    public class IdAttributeTests
    {
        public class EqualsOverride : IdAttributeTests
        {
            [Theory]
            [InlineData("foo", true)]
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

#nullable enable
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
#nullable restore
    }
}
