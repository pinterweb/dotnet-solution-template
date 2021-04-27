using Xunit;

namespace BusinessApp.Analyzers.IntegrationTest
{
    public class IdAttributeTests
    {
        public class IEquatableImpl : IdAttributeTests
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
                object other = new EntityStub { Id = b };

                /* Act */
                var areEqual = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectTrue, areEqual);
            }
        }

        public class IComparableImpl : IdAttributeTests
        {
            [Theory]
            [InlineData(1, 1)]
            [InlineData(2, 0)]
            [InlineData(3, -1)]
            public void WhenCompared_AllPropsCompared(int b, int returnVal)
            {
                /* Arrange */
                var sut = new StructStub { Id = 2 };
                var other = new StructStub { Id = b };

                /* Act */
                var compareValue = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(returnVal, compareValue);
            }
        }

        public class OperatorOverrides : IdAttributeTests
        {
            [Theory]
            [InlineData(1, true)]
            [InlineData(2, false)]
            public void Equals_WhenPropertiesEqual_TrueReturned(int b, bool expectTrue)
            {
                /* Arrange */
                var sut = new StructStub { Id = 1 };
                var other = new StructStub { Id = b };

                /* Act */
                var areEqual = sut == other;

                /* Assert */
                Assert.Equal(expectTrue, areEqual);
            }

            [Theory]
            [InlineData(1, false)]
            [InlineData(2, true)]
            public void NotEquals_WhenPropertiesNotEqual_TrueReturned(int b, bool expectTrue)
            {
                /* Arrange */
                var sut = new StructStub { Id = 1 };
                var other = new StructStub { Id = b };

                /* Act */
                var areEqual = sut != other;

                /* Assert */
                Assert.Equal(expectTrue, areEqual);
            }

            [Theory]
            [InlineData(1, false)]
            [InlineData(2, false)]
            [InlineData(3, true)]
            public void LessThan_WhenPropertyLessThan_TrueReturned(int b, bool expectTrue)
            {
                /* Arrange */
                var sut = new StructStub { Id = 2 };
                var other = new StructStub { Id = b };

                /* Act */
                var isTrue = sut < other;

                /* Assert */
                Assert.Equal(expectTrue, isTrue);
            }

            [Theory]
            [InlineData(1, true)]
            [InlineData(2, false)]
            [InlineData(3, false)]
            public void GreaterThan_WhenPropertyGreaterThan_TrueReturned(int b, bool expectTrue)
            {
                /* Arrange */
                var sut = new StructStub { Id = 2 };
                var other = new StructStub { Id = b };

                /* Act */
                var isTrue = sut > other;

                /* Assert */
                Assert.Equal(expectTrue, isTrue);
            }

            [Theory]
            [InlineData(1, false)]
            [InlineData(2, true)]
            [InlineData(3, true)]
            public void LessThanOrEqualTo_WhenPropertyLessThanOrEqualTo_TrueReturned(int b, bool expectTrue)
            {
                /* Arrange */
                var sut = new StructStub { Id = 2 };
                var other = new StructStub { Id = b };

                /* Act */
                var isTrue = sut <= other;

                /* Assert */
                Assert.Equal(expectTrue, isTrue);
            }

            [Theory]
            [InlineData(1, true)]
            [InlineData(2, true)]
            [InlineData(3, false)]
            public void GreaterThanOrEqualTo_WhenPropertyGreaterThanOrEqualTo_TrueReturned(int b, bool expectTrue)
            {
                /* Arrange */
                var sut = new StructStub { Id = 2 };
                var other = new StructStub { Id = b };

                /* Act */
                var isTrue = sut >= other;

                /* Assert */
                Assert.Equal(expectTrue, isTrue);
            }
        }
    }
}
