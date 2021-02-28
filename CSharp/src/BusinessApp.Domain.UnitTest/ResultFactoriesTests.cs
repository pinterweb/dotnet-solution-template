namespace BusinessApp.Domain.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class ResultFactoriesTests
    {
        public class NotNull : ResultFactoriesTests
        {
            [Theory]
            [InlineData(null, ValueKind.Error)]
            [InlineData("", ValueKind.Ok)]
            [InlineData("foo", ValueKind.Ok)]
            public void ValueNull_ErrorKindReturned(string val, ValueKind expected)
            {
                /* Act */
                var result = val.NotNull();

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Theory]
            [InlineData(null)]
            public void ValueNull_ErrorMessageInResult(string val)
            {
                /* Act */
                var result = val.NotNull();

                /* Assert */
                var ex = Record.Exception(result.Unwrap);
                Assert.Equal("Value cannot be null", ex?.Message);
            }

            [Fact]
            public void ValueNotNull_OriginalValueReturned()
            {
                /* Arrange */
                var val = "foo";

                /* Act */
                var result = val.NotNull();

                /* Assert */
                Assert.Equal(val, result.Unwrap());
            }
        }

        public class NotEmpty : ResultFactoriesTests
        {
            public static IEnumerable<object[]> EnumerableKindArgs => new[]
            {
                new object[] { null, ValueKind.Error },
                new object[] { new List<int>(), ValueKind.Error },
                new object[] { new List<int> { 1 }, ValueKind.Ok }
            };

            public static IEnumerable<object[]> EnumerableMsgArgs => new[]
            {
                new object[] { null, "Value cannot be null" },
                new object[] { new List<int>(), "Collection cannot be empty" },
            };

            [Theory]
            [InlineData(null, ValueKind.Error)]
            [InlineData("", ValueKind.Error)]
            [InlineData("foo", ValueKind.Ok)]
            public void StringEmpty_ErrorKindReturned(string val, ValueKind expected)
            {
                /* Act */
                var result = val.NotEmpty();

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Theory]
            [InlineData(null, "Value cannot be null")]
            [InlineData("", "String value cannot be empty")]
            public void StringEmpty_ErrorMessageReturned(string val, string expectedErrMsg)
            {
                /* Act */
                var result = val.NotEmpty();

                /* Assert */
                var ex = Record.Exception(result.Unwrap);
                Assert.Equal(expectedErrMsg, ex.Message);
            }

            [Fact]
            public void StringNotEmpty_OriginalValueReturned()
            {
                /* Arrange */
                var val = "foo";

                /* Act */
                var result = val.NotEmpty();

                /* Assert */
                Assert.Equal(val, result.Unwrap());
            }

            [Theory, MemberData(nameof(EnumerableKindArgs))]
            public void EnumerableEmpty_ErrorKindReturned(IEnumerable<int> vals, ValueKind expected)
            {
                /* Act */
                var result = vals.NotEmpty();

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Theory, MemberData(nameof(EnumerableMsgArgs))]
            public void EnumerableEmpty_ErrorMessageReturned(IEnumerable<int> vals,
                string expectedErrMsg)
            {
                /* Act */
                var result = vals.NotEmpty();

                /* Assert */
                var ex = Record.Exception(result.Unwrap);
                Assert.Equal(expectedErrMsg, ex.Message);
            }

            [Fact]
            public void EnumerableNotEmpty_OriginalValueReturned()
            {
                /* Arrange */
                var vals = new[] { "foo" };

                /* Act */
                var result = vals.NotEmpty();

                /* Assert */
                Assert.Same(vals, result.Unwrap());
            }
        }
        public class NotDefault : ResultFactoriesTests
        {
            [Theory]
            [InlineData(0, ValueKind.Error)]
            [InlineData(1, ValueKind.Ok)]
            [InlineData(-1, ValueKind.Ok)]
            public void ValueIsDefault_ErrorKindReturned(int val, ValueKind expected)
            {
                /* Act */
                var result = val.NotDefault();

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Theory]
            [InlineData(null, "Value cannot be equal to 'null'")]
            public void NullableValueIsNull_ErrorMessageReturned(int? val, string expectedErrMsg)
            {
                /* Act */
                var result = val.NotDefault();

                /* Assert */
                var ex = Record.Exception(() => result.Unwrap());
                Assert.Equal(expectedErrMsg, ex.Message);
            }

            [Theory]
            [InlineData(0, "Value cannot be equal to '0'")]
            public void NonNullableValueIsDefault_ErrorMessageReturned(int val, string expectedErrMsg)
            {
                /* Act */
                var result = val.NotDefault();

                /* Assert */
                var ex = Record.Exception(() => result.Unwrap());
                Assert.Equal(expectedErrMsg, ex.Message);
            }

            [Fact]
            public void ValueNotDefault_OriginalValueReturned()
            {
                /* Arrange */
                int val = 1;

                /* Act */
                var result = val.NotDefault();

                /* Assert */
                Assert.Equal(val, result.Unwrap());
            }
        }

        public class True : ResultFactoriesTests
        {
            [Theory]
            [InlineData(DateTimeKind.Utc, ValueKind.Error)]
            [InlineData(DateTimeKind.Local, ValueKind.Ok)]
            public void IsFalse_ErrorKindReturned(DateTimeKind kind, ValueKind expected)
            {
                /* Arrange */
                var dt = new DateTime(100, kind);

                /* Act */
                var result = dt.Valid(dt.Kind != DateTimeKind.Utc);

                /* Assert */
                Assert.Equal(expected, result.Kind);
            }

            [Fact]
            public void IsFalse_ErrorMessageReturned()
            {
                /* Arrange */
                var dt = new DateTime(100, DateTimeKind.Utc);

                /* Act */
                var result = dt.Valid(dt.Kind != DateTimeKind.Utc);

                /* Assert */
                Assert.Equal("Test did not pass", result.UnwrapError().ToString());
            }

            [Fact]
            public void IsTrue_OriginalValueReturned()
            {
                /* Arrange */
                var dt = new DateTime(100, DateTimeKind.Local);

                /* Act */
                var result = dt.Valid(dt.Kind != DateTimeKind.Utc);

                /* Assert */
                Assert.Equal(dt, result.Unwrap());
            }
        }
    }
}
