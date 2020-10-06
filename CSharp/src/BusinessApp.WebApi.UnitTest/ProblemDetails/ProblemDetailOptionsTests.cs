namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    using Xunit;
    using BusinessApp.WebApi.ProblemDetails;
    using System;

    public class ProblemDetailOptionsTests
    {
        public readonly ProblemDetailOptions sut;

        public class ObjectEqualsOverride : ProblemDetailOptionsTests
        {
            [Fact]
            public void NotAProblemDetailOptiontArgument_AreEqual()
            {
                /* Arrange */
                var sut = new ProblemDetailOptions();

                /* Act */
                bool equal = sut.Equals("foo");

                /* Assert */
                Assert.False(equal);
            }

            [Fact]
            public void HaveSameProblemType_AreEqual()
            {
                /* Arrange */
                var sut = new ProblemDetailOptions
                {
                    ProblemType = typeof(string)
                };
                var other = new ProblemDetailOptions
                {
                    ProblemType = typeof(string)
                };

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.True(equal);
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
                var sut = new ProblemDetailOptions
                {
                    ProblemType = a
                };
                var other = new ProblemDetailOptions
                {
                    ProblemType = b
                };

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }
        }
    }
}
