namespace ShelLife.Domain.UnitTest
{
    using Xunit;
    using BusinessApp.Domain;

    public class InvariantsTests
    {
        public class NotNull : InvariantsTests
        {
            [Theory]
            [InlineData(null, Result.Failure)]
            [InlineData("", Result.Ok)]
            [InlineData("foo", Result.Ok)]
            public void ValueIsNull_FailureResultReturned(string val, Result expected)
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
            [InlineData(null, Result.Failure)]
            [InlineData("", Result.Failure)]
            [InlineData("foo", Result.Ok)]
            public void ValueIsEmpty_FailureResultReturned(string val, Result expected)
            {
                /* Act */
                var result = Invariants.NotEmpty(val);

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }
        }
    }
}
