namespace ShelLife.Domain.UnitTest
{
    using Xunit;
    using BusinessApp.Domain;
    using System.Collections.Generic;

    public partial class GuardAgainstTests
    {
        public class Against : GuardAgainstTests
        {
            public static IEnumerable<object[]> EnumerableKindArgs => new[]
            {
                new object[] { null, Result.Error },
                new object[] { new List<int>(), Result.Error },
                new object[] { new List<int> { 1 }, Result.Ok }
            };

            public static IEnumerable<object[]> EnumerableMsgArgs => new[]
            {
                new object[] { null, "Value cannot be null" },
                new object[] { new List<int>(), "Collection cannot be empty" },
            };

            [Theory]
            [InlineData(null, Result.Error)]
            [InlineData("", Result.Ok)]
            [InlineData("foo", Result.Ok)]
            public void Null_WhenValueNull_ErrorKindReturned(string val, Result expected)
            {
                /* Act */
                var result = Guard.Against.Null(val);

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Theory]
            [InlineData(null)]
            public void Null_WhenValueNull_ErrorMessageInResult(string val)
            {
                /* Act */
                var result = Guard.Against.Null(val);

                /* Assert */
                var ex = Record.Exception(result.Unwrap);
                Assert.Equal("Value cannot be null", ex?.Message);
            }

            [Theory]
            [InlineData(null, Result.Error)]
            [InlineData("", Result.Error)]
            [InlineData("foo", Result.Ok)]
            public void StringEmpty_WhenValueIsEmpty_ErrorKindReturned(string val, Result expected)
            {
                /* Act */
                var result = Guard.Against.Empty(val);

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Theory]
            [InlineData(null, "Value cannot be null")]
            [InlineData("", "String value cannot be empty")]
            public void StringEmpty_WhenValueIsEmpty_ErrorMessageReturned(string val, string expectedErrMsg)
            {
                /* Act */
                var result = Guard.Against.Empty(val);

                /* Assert */
                var ex = Record.Exception(result.Unwrap);
                Assert.Equal(expectedErrMsg, ex.Message);
            }

            [Theory]
            [InlineData(0, Result.Error)]
            [InlineData(1, Result.Ok)]
            [InlineData(-1, Result.Ok)]
            public void Defalt_WhenValueIsDefault_ErrorKindReturned(int val, Result expected)
            {
                /* Act */
                var result = Guard.Against.Default(val);

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Theory]
            [InlineData(null, "Value cannot be equal to 'null'")]
            public void Default_WithNullableValueIsNull_ErrorMessageReturned(int? val, string expectedErrMsg)
            {
                /* Act */
                var result = Guard.Against.Default(val);

                /* Assert */
                var ex = Record.Exception(() => result.Unwrap());
                Assert.Equal(expectedErrMsg, ex.Message);
            }

            [Theory]
            [InlineData(0, "Value cannot be equal to '0'")]
            public void Default_WithNonNullableValueIsDefault_ErrorMessageReturned(int val, string expectedErrMsg)
            {
                /* Act */
                var result = Guard.Against.Default(val);

                /* Assert */
                var ex = Record.Exception(() => result.Unwrap());
                Assert.Equal(expectedErrMsg, ex.Message);
            }

            [Theory, MemberData(nameof(EnumerableKindArgs))]
            public void EnumerableEmpty_WhenValueIsEmpty_ErrorKindReturned(IEnumerable<int> vals, Result expected)
            {
                /* Act */
                var result = Guard.Against.Empty(vals);

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Theory, MemberData(nameof(EnumerableMsgArgs))]
            public void EnumerableEmpty_WhenValueIsEmpty_ErrorMessageReturned(IEnumerable<int> vals, string expectedErrMsg)
            {
                /* Act */
                var result = Guard.Against.Empty(vals);

                /* Assert */
                var ex = Record.Exception(result.Unwrap);
                Assert.Equal(expectedErrMsg, ex.Message);
            }

        }
    }
}
