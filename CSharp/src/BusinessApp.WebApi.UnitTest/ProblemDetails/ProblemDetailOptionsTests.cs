namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    using Xunit;
    using BusinessApp.WebApi.ProblemDetails;
    using System;
    using FakeItEasy;

    public class ProblemDetailOptionsTests
    {
        public readonly ProblemDetailOptions sut;

        public class Constructor : ProblemDetailOptionsTests
        {
            [Fact]
            public void SetsType()
            {
                /* Arrange */
                var type = typeof(string);

                /* Act */
                var sut = new ProblemDetailOptions(typeof(string), 0);

                /* Assert */
                Assert.Equal(type, sut.ProblemType);
            }

            [Fact]
            public void SetsStatusCode()
            {
                /* Arrange */
                var status = 2;

                /* Act */
                var sut = new ProblemDetailOptions(A.Dummy<Type>(), status);

                /* Assert */
                Assert.Equal(2, sut.StatusCode);
            }
        }

        public class ObjectEqualsOverride : ProblemDetailOptionsTests
        {
            [Fact]
            public void NotAProblemDetailArgument_NotEqual()
            {
                /* Arrange */
                var sut = new ProblemDetailOptions(A.Dummy<Type>(), 0);

                /* Act */
                bool equal = sut.Equals("foo");

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [InlineData(typeof(string), typeof(string), true)]
            [InlineData(null, typeof(string), false)]
            [InlineData(typeof(string), null, false)]
            [InlineData(null, null, false)]
            public void UsesProblemTypeProperty_TrueReturnedWhenEqual(Type a, Type b,
                bool expectEquals)
            {
                /* Arrange */
                var sut = new ProblemDetailOptions(a, 0);
                var other = new ProblemDetailOptions(b, 1);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }
        }
    }
}
