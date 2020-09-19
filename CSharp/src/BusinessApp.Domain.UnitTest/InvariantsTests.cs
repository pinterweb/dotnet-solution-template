namespace ShelLife.Domain.UnitTest
{
    using Xunit;
    using BusinessApp.Domain;

    public class InvariantsTests
    {
        public class NotNull : InvariantsTests
        {
            [Theory]
            [InlineData(null, Result.Error)]
            [InlineData("", Result.Ok)]
            [InlineData("foo", Result.Ok)]
            public void ValueIsNull_ErrorResultReturned(string val, Result expected)
            {
                /* Act */
                var result = Invariants.NotNull(val);

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }
        }

        public class NotEmptyString : InvariantsTests
        {
            [Theory]
            [InlineData(null, Result.Error)]
            [InlineData("", Result.Error)]
            [InlineData("foo", Result.Ok)]
            public void ValueIsEmpty_ErrorResultReturned(string val, Result expected)
            {
                /* Act */
                var result = Invariants.NotEmpty(val);

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }
        }

        public class Test : InvariantsTests
        {
            [Theory]
            [InlineData("", Result.Error)]
            [InlineData("foo", Result.Ok)]
            public void ValueTestedAgainstExpression_FailedWhenFalse(string val, Result expected)
            {
                /* Act */
                var result = Invariants.Test(val, val => val == "foo");

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }
        }
    }
}
