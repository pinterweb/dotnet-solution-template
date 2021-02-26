using Xunit;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using _ = System.Int16;

namespace BusinessApp.Domain.UnitTest
{
    public class TestData
    {
        public static IEnumerable<object[]> IFormattableArgs => new[]
        {
            new object[] { FormattableStringFactory.Create("foo"), FormattableStringFactory.Create("foo"), true },
            new object[] { FormattableStringFactory.Create("foo"), FormattableStringFactory.Create("bar"), false },
            new object[] { 1, 2, false },
            new object[] { 2, 2, true }
        };
    }

    public class ResultTests
    {
        public class FromFactory : ResultTests
        {
            [Fact]
            public void FuncDoesNotThrow_IsOk()
            {
                /* Arrange */
                Func<string> noop = () => "foo";

                /* Act */
                var result = Result.From(noop);

                /* Assert */
                Assert.Equal(Result<string, IFormattable>.Ok("foo"), result);
            }

            [Fact]
            public void FuncThrowsFormattable_ErrorReturned()
            {
                /* Arrange */
                var error = A.Fake<Exception>(opt => opt.Implements<IFormattable>());
                Func<string> noop = () => throw error;

                /* Act */
                var result = Result.From(noop);

                /* Assert */
                Assert.Same(error, result.UnwrapError());
            }

            [Fact]
            public void FuncThrowsNotFormattable_ExceptionThrown()
            {
                /* Arrange */
                var error = A.Dummy<Exception>();
                Func<string> noop = () => throw error;

                /* Act */
                var ex = Record.Exception(() => Result.From(noop));

                /* Assert */
                Assert.Same(error, ex);
            }
        }

        public class OkFactory : ResultTests
        {
            [Fact]
            public void KindProperty_IsOk()
            {
                /* Act */
                var sut = Result.Ok;

                /* Assert */
                Assert.Equal(ValueKind.Ok, sut.Kind);
            }
        }

        public class ErrorFactory : ResultTests
        {
            [Fact]
            public void KindProperty_IsError()
            {
                /* Arrange */
                var value = A.Dummy<IFormattable>();

                /* Act */
                var sut = Result.Error(value);

                /* Assert */
                Assert.Equal(ValueKind.Error, sut.Kind);
            }
        }

        public class Into : ResultTests
        {
            [Fact]
            public void OkValueAsIFormattable_EmptyStringReturned()
            {
                /* Act */
                var sut = Result.Ok;

                /* Assert */
                Assert.Equal(Result<IFormattable, IFormattable>.Ok(null), sut.Into());
            }

            [Fact]
            public void ErrorValueAsIFormattable_InnerErrorReturned()
            {
                /* Arrange */
                IFormattable errVal = $"foobar";

                /* Act */
                var sut = Result.Error(errVal);

                /* Assert */
                Assert.Equal(Result<IFormattable, IFormattable>.Error(errVal), sut.Into());
            }

            [Fact]
            public void GenericValue_DefaultValueUsed()
            {
                /* Act */
                var sut = Result.Ok;

                /* Assert */
                Assert.Equal(Result<string, IFormattable>.Ok(null), sut.Into<string>());
            }

            [Fact]
            public void GenericResult_InnerErrorReturned()
            {
                /* Arrange */
                IFormattable errVal = $"foobar";

                /* Act */
                var sut = Result.Error(errVal);

                /* Assert */
                Assert.Equal(Result<string, IFormattable>.Error(errVal), sut.Into<string>());
            }
        }

        public class IEquatableEquals : ResultTests
        {
            [Fact]
            public void BothOkResults_AreEqual()
            {
                /* Arrange */
                Result sut = Result.Ok;
                Result other = Result.Ok;

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.True(equal);
            }

            [Fact]
            public void ResultsHaveDifferentKind_AreNotEqual()
            {
                /* Arrange */
                Result sut = Result.Ok;
                Result other = Result.Error($"");

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [MemberData(nameof(TestData.IFormattableArgs), MemberType = typeof(TestData))]
            public void ResultsAreErrorKind_InnerValueCompared(IFormattable a, IFormattable b, bool expectEquals)
            {
                /* Arrange */
                Result sut = Result.Error(a);
                Result other = Result.Error(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }
        }

        public class ObjectOverrideEquals : ResultTests
        {
            [Fact]
            public void NotAResultArgument_AreEqual()
            {
                /* Arrange */
                Result sut = Result.Ok;

                /* Act */
                bool equal = sut.Equals("foo");

                /* Assert */
                Assert.False(equal);
            }

            [Fact]
            public void BothOkResults_AreEqual()
            {
                /* Arrange */
                Result sut = Result.Ok;
                object other = Result.Ok;

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.True(equal);
            }

            [Fact]
            public void ResultsHaveDifferentKind_AreNotEqual()
            {
                /* Arrange */
                Result sut = Result.Ok;
                object other = Result.Error($"");

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [MemberData(nameof(TestData.IFormattableArgs), MemberType = typeof(TestData))]
            public void ResultsAreaErrorKind_InnerValueCompared(IFormattable a, IFormattable b, bool expectEquals)
            {
                /* Arrange */
                Result sut = Result.Error(a);
                object other = Result.Error(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }
        }

        public class CompareTo
        {
            [Fact]
            public void BothOkResults_ZeroReturned()
            {
                /* Arrange */
                Result sut = Result.Ok;
                Result other = Result.Ok;

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(0, order);
            }

            [Fact]
            public void ArgumentResultIsErrorAndInstanceIsOk_NegativeOneReturnedj()
            {
                /* Arrange */
                Result sut = Result.Ok;
                Result other = Result.Error($"");

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(-1, order);
            }

            [Fact]
            public void ArgumentResultIsOkAndInstanceIsError_OneReturned()
            {
                /* Arrange */
                Result sut = Result.Error($"");
                Result other = Result.Ok;

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(1, order);
            }

            [Theory]
            [InlineData(1, 1, 0)]
            [InlineData(1, 2, -1)]
            [InlineData(2, 1, 1)]
            public void BothResultsAreErrorKind_InnerValueCompared(int a, int b,
                int expectedOrder)
            {
                /* Arrange */
                Result sut = Result.Error(a);
                Result other = Result.Error(b);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(order, expectedOrder);
            }
        }

        public class ToStringOverride : ResultTests
        {
            [Fact]
            public void OkResult_ReturnsEmptyString()
            {
                /* Arrange */
                var sut = Result.Ok;

                /* Act */
                var str = sut.ToString();

                /* Assert */
                Assert.Equal("", str);
            }

            [Fact]
            public void ErrResult_CallsToString()
            {
                /* Arrange */
                var formattable = A.Fake<IFormattable>();
                var sut = Result.Error(formattable);
                A.CallTo(() => formattable.ToString("G", null)).Returns("foobar");

                /* Act */
                var str = sut.ToString();

                /* Assert */
                Assert.Equal("foobar", str);
            }
        }

        public class IFormattableToString : ResultTests
        {
            [Fact]
            public void OkResult_ReturnsEmptyString()
            {
                /* Arrange */
                var sut = Result.Ok;

                /* Act */
                var str = sut.ToString();

                /* Assert */
                Assert.Equal("", str);
            }

            [Fact]
            public void ErrResult_CallsToString()
            {
                /* Arrange */
                var formattable = A.Fake<IFormattable>();
                var provider = A.Fake<IFormatProvider>();
                var sut = Result.Error(formattable);
                A.CallTo(() => formattable.ToString("z", provider)).Returns("foobar");

                /* Act */
                var str = sut.ToString("z", provider);

                /* Assert */
                Assert.Equal("foobar", str);
            }
        }
    }

    public class ResultOfTTests
    {
        public class OkFactory : ResultOfTTests
        {
            [Fact]
            public void KindProperty_IsOk()
            {
                /* Arrange */
                string value = A.Dummy<string>();

                /* Act */
                var sut = Result<string>.Ok(A.Dummy<string>());

                /* Assert */
                Assert.Equal(ValueKind.Ok, sut.Kind);
            }
        }

        public class ErrorFactory : ResultOfTTests
        {
            [Fact]
            public void KindProperty_IsError()
            {
                /* Arrange */
                var value = A.Dummy<IFormattable>();

                /* Act */
                var sut = Result<_>.Error(value);

                /* Assert */
                Assert.Equal(ValueKind.Error, sut.Kind);
            }
        }

        public class Into : ResultOfTTests
        {
            [Fact]
            public void OkValueAsIFormattable_EmptyStringReturned()
            {
                /* Act */
                var sut = Result<int>.Ok(1);

                /* Assert */
                Assert.Equal(Result<int, IFormattable>.Ok(1), sut.Into());
            }

            [Fact]
            public void ErrorValueAsIFormattable_InnerErrorReturned()
            {
                /* Arrange */
                IFormattable errVal = $"foobar";

                /* Act */
                var sut = Result<int>.Error(errVal);

                /* Assert */
                Assert.Equal(Result<int, IFormattable>.Error(errVal), sut.Into());
            }
        }

        public class IEquatableEquals : ResultOfTETests
        {
            [Theory]
            [InlineData(1, 1, true)]
            [InlineData(2, 1, false)]
            public void BothOkResults_AreEqualWhenValuesEqual(int a, int b, bool expectEquals)
            {
                /* Arrange */
                var sut = Result<int>.Ok(a);
                var other = Result<int>.Ok(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }

            [Fact]
            public void ResultsHaveDifferentKind_AreNotEqual()
            {
                /* Arrange */
                var sut = Result<int>.Ok(1);
                var other = Result<int>.Error(2);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [MemberData(nameof(TestData.IFormattableArgs), MemberType = typeof(TestData))]
            public void BothErrorResults_AreEqualWhenValuesEqual(IFormattable a, IFormattable b, bool expectEquals)
            {
                /* Arrange */
                var sut = Result<int>.Error(a);
                var other = Result<int>.Error(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }
        }

        public class ObjectOverrideEquals : ResultOfTETests
        {
            [Fact]
            public void NotAResultArgument_AreEqual()
            {
                /* Arrange */
                var sut = Result<int>.Ok(1);

                /* Act */
                bool equal = sut.Equals("foo");

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [InlineData(1, 1, true)]
            [InlineData(2, 1, false)]
            public void BothOkResults_AreEqualWhenValuesEqual(int a, int b, bool expectEquals)
            {
                /* Arrange */
                var sut = Result<int>.Ok(a);
                object other = Result<int>.Ok(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }

            [Fact]
            public void ResultsHaveDifferentKind_AreNotEqual()
            {
                /* Arrange */
                var sut = Result<int>.Ok(1);
                object other = Result<int>.Error(2);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [MemberData(nameof(TestData.IFormattableArgs), MemberType = typeof(TestData))]
            public void BothErrorResults_AreEqualWhenValuesEqual(IFormattable a, IFormattable b, bool expectEquals)
            {
                /* Arrange */
                var sut = Result<int>.Error(a);
                object other = Result<int>.Error(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }
        }

        public class CompareTo
        {
            [Theory]
            [InlineData(1, 1, 0)]
            [InlineData(1, 2, -1)]
            [InlineData(2, 1, 1)]
            public void BothOkResults_InnerValueCompared(int a, int b, int expectedOrder)
            {
                /* Arrange */
                var sut = Result<int>.Ok(a);
                var other = Result<int>.Ok(b);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(expectedOrder, order);
            }

            [Fact]
            public void ArgumentResultIsErrorAndInstanceIsOk_NegativeOneReturnedj()
            {
                /* Arrange */
                var sut = Result<int>.Ok(1);
                var other = Result<int>.Error(1);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(-1, order);
            }

            [Fact]
            public void ArgumentResultIsOkAndInstanceIsError_OneReturned()
            {
                /* Arrange */
                var sut = Result<int>.Error(1);
                var other = Result<int>.Ok(1);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(1, order);
            }

            [Theory]
            [InlineData(1, 1, 0)]
            [InlineData(1, 2, -1)]
            [InlineData(2, 1, 1)]
            public void BothResultsAreErrorKind_InnerValueCompared(int a, int b,
                int expectedOrder)
            {
                /* Arrange */
                var sut = Result<int>.Error(a);
                var other = Result<int>.Error(b);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(order, expectedOrder);
            }
        }

        public class ImplicitCastToResult : ResultOfTTests
        {
            [Fact]
            public void WhenOk_OkResultReturned()
            {
                /* Arrange */
                var sut = Result<int>.Ok(1);

                /* Act */
                Result cast = sut;

                /* Assert */
                Assert.Equal(Result.Ok, cast);
            }

            [Fact]
            public void WhenError_ErrorResultReturned()
            {
                /* Arrange */
                var error = A.Fake<IFormattable>();
                A.CallTo(() => error.ToString(null, null)).Returns("Foo");
                var sut = Result<int>.Error(error);

                /* Act */
                Result cast = sut;

                /* Assert */
                Assert.Equal(Result.Error($"Foo"), cast);
            }
        }

        public class ImplicitCastToResultOfTE : ResultOfTTests
        {
            [Fact]
            public void WhenOk_OkResultReturned()
            {
                /* Arrange */
                var sut = Result<int>.Ok(1);

                /* Act */
                Result<int, IFormattable> cast = sut;

                /* Assert */
                Assert.Equal(Result<int, IFormattable>.Ok(1), cast);
            }

            [Fact]
            public void WhenError_ErrorResultReturned()
            {
                /* Arrange */
                var error = A.Fake<IFormattable>();
                A.CallTo(() => error.ToString(null, null)).Returns("Foo");
                var sut = Result<int>.Error(error);

                /* Act */
                Result<int, IFormattable> cast = sut;

                /* Assert */
                Assert.Equal(Result<int, IFormattable>.Error($"Foo"), cast);
            }
        }
    }

    public class ResultOfTETests
    {
        public class OkFactory : ResultOfTETests
        {
            [Fact]
            public void KindProperty_IsOk()
            {
                /* Arrange */
                string value = A.Dummy<string>();

                /* Act */
                var sut = Result<string, _>.Ok(A.Dummy<string>());

                /* Assert */
                Assert.Equal(ValueKind.Ok, sut.Kind);
            }
        }

        public class ErrorFactory : ResultOfTETests
        {
            [Fact]
            public void KindProperty_IsError()
            {
                /* Arrange */
                var value = A.Dummy<IFormattable>();

                /* Act */
                var sut = Result<_, IFormattable>.Error(value);

                /* Assert */
                Assert.Equal(ValueKind.Error, sut.Kind);
            }
        }

        public class ExplicitCastToValue
        {
            [Fact]
            public void ResultIsValue_ValueReturned()
            {
                /* Arrange */
                string value = "foo";

                /* Act */
                var sut = Result<string, _>.Ok(value);

                /* Assert */
                Assert.Same(value, (string)sut);
            }

            [Fact]
            public void ResultIsError_BadStateExceptionThrown()
            {
                /* Arrange */
                IFormattable value = $"foo";
                var sut = Result<_, IFormattable>.Error(value);

                /* Act */
                var ex = Record.Exception(() => (_)sut);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "Cannot get the value because it is an error: foo",
                    ex.Message);
            }
        }

        public class Expect : ResultOfTETests
        {
            [Fact]
            public void IsErrorResult_BadStateExceptionThrown()
            {
                /* Arrange */
                IFormattable error = $"foo";
                var sut = Result<_, IFormattable>.Error(error);

                /* Act */
                var ex = Record.Exception(() => sut.Expect("Some message"));

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "Some message: foo",
                    ex.Message);
            }

            [Fact]
            public void IsOkResult_ValueReturned()
            {
                /* Arrange */
                var value = "foo";
                var sut = Result<string, IFormattable>.Ok(value);

                /* Act */
                var okVal = sut.Expect("Some message");

                /* Assert */
                Assert.Equal("foo", okVal);
            }

            [Fact]
            public void ExpectExceptionWithoutCorrectCtor_ExceptionThrown()
            {
                /* Arrange */
                IFormattable value = $"foobar";
                var sut = Result<_, IFormattable>.Error(value);

                /* Act */
                var ex = Record.Exception(() => sut.Expect<InvalidFooException>("Some message"));

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    $"'InvalidFooException' does not have a constructor with string parameter. " +
                    $"Original error value is: 'foobar'",
                    ex.Message);
            }

            [Fact]
            public void ExpectExceptionWithCorrectCtor_ExceptionThrown()
            {
                /* Arrange */
                IFormattable value = $"foobar";
                var sut = Result<_, IFormattable>.Error(value);

                /* Act */
                var ex = Record.Exception(() => sut.Expect<ValidFooException>("Some message"));

                /* Assert */
                Assert.IsType<ValidFooException>(ex);
                Assert.Equal(
                    "Some message: foobar",
                    ex.Message);
            }
        }

        public class ExpectError : ResultOfTETests
        {
            [Fact]
            public void IsOkResult_BadStateExceptionThrown()
            {
                /* Arrange */
                var value = $"foo";
                var sut = Result<string, _>.Ok(value);

                /* Act */
                var ex = Record.Exception(() => sut.ExpectError("Some message"));

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "Some message: foo",
                    ex.Message);
            }

            [Fact]
            public void IsErrorResult_ErrorReturned()
            {
                /* Arrange */
                FormattableString error = $"foo";
                var sut = Result<_, FormattableString>.Error(error);

                /* Act */
                var errVal = sut.ExpectError(A.Dummy<string>());

                /* Assert */
                Assert.Equal("foo", FormattableString.Invariant(errVal));
            }
        }

        public class UnwrapError : ResultOfTETests
        {
            [Fact]
            public void IsOkResult_BadStateExceptionThrown()
            {
                /* Arrange */
                var value = $"foo";
                var sut = Result<string, _>.Ok(value);

                /* Act */
                var ex = Record.Exception(() => sut.UnwrapError());

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "foo",
                    ex.Message);
            }

            [Fact]
            public void IsErrorResult_ErrorReturned()
            {
                /* Arrange */
                FormattableString error = $"foo";
                var sut = Result<_, FormattableString>.Error(error);

                /* Act */
                var errVal = sut.UnwrapError();

                /* Assert */
                Assert.Equal("foo", FormattableString.Invariant(errVal));
            }
        }

        public class Unwrap : ResultOfTETests
        {
            [Fact]
            public void IsErrorResult_BadStateExceptionThrown()
            {
                /* Arrange */
                FormattableString error = $"foo";
                var sut = Result<_, FormattableString>.Error(error);

                /* Act */
                var ex = Record.Exception(() => sut.Unwrap());

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void IsOkResult_ValueReturned()
            {
                /* Arrange */
                var value = $"foo";
                var sut = Result<string, _>.Ok(value);

                /* Act */
                var okVal = sut.Unwrap();

                /* Assert */
                Assert.Equal("foo", okVal);
            }
        }

        public class AndThen : ResultOfTETests
        {
            [Fact]
            public void WhenOk_NextFunctionCalledOnce()
            {
                /* Arrange */
                var value = "foo";
                var sut = Result<string, _>.Ok(value);
                var expectResult = Result<string, _>.Ok("bar");
                Func<string, Result<string, _>> next = (result) => expectResult;

                /* Act */
                var nextResult = sut.AndThen(next);

                /* Assert */
                Assert.Equal(expectResult, nextResult);
            }

            [Fact]
            public void WhenErr_SelfReturned()
            {
                /* Arrange */
                IFormattable value = $"foo";
                var sut = Result<_, IFormattable>.Error(value);
                Func<_, Result<_, IFormattable>> next = (result) => A.Dummy<Result<_, IFormattable>>();

                /* Act */
                var nextResult = sut.AndThen(next);

                /* Assert */
                Assert.Equal(sut, nextResult);
            }
        }

        public class ThenRun : ResultOfTETests
        {
            [Fact]
            public void FuncOfTR_WhenOk_NextFunctionCalledOnce()
            {
                /* Arrange */
                int called = 0;
                var value = "foo";
                var sut = Result<string, _>.Ok(value);
                Func<string, string> next = (result) => { ++called; return "bar"; };

                /* Act */
                var nextResult = sut.ThenRun(next);

                /* Assert */
                Assert.Equal(1, called);
            }

            [Fact]
            public void FuncOfTR_WhenOk_SelfReturned()
            {
                /* Arrange */
                var value = "foo";
                var sut = Result<string, _>.Ok(value);
                Func<string, string> next = (result) => "bar";

                /* Act */
                var nextResult = sut.ThenRun(next);

                /* Assert */
                Assert.Equal(sut, nextResult);
            }

            [Fact]
            public void FuncOfTR_WhenErr_NextFunctionNotCalled()
            {
                /* Arrange */
                int called = 0;
                var sut = Result<string, _>.Error(1);
                Func<string, string> next = (result) => { ++called; return "bar"; };

                /* Act */
                var nextResult = sut.ThenRun(next);

                /* Assert */
                Assert.Equal(0, called);
            }

            [Fact]
            public void FuncOfTR_WhenErr_SelfReturned()
            {
                /* Arrange */
                var sut = Result<string, _>.Error(1);
                Func<string, string> next = (result) => "bar";

                /* Act */
                var nextResult = sut.ThenRun(next);

                /* Assert */
                Assert.Equal(sut, nextResult);
            }

            [Fact]
            public void Action_WhenOk_NextFunctionCalledOnce()
            {
                /* Arrange */
                int called = 0;
                var sut = Result<string, _>.Ok("foo");
                Action<string> next = (result) => called++;

                /* Act */
                var nextResult = sut.ThenRun(next);

                /* Assert */
                Assert.Equal(1, called);
            }

            [Fact]
            public void Action_WhenOk_SelfReturned()
            {
                /* Arrange */
                var sut = Result<string, _>.Ok("foo");

                /* Act */
                var nextResult = sut.ThenRun(A.Dummy<Action<string>>());

                /* Assert */
                Assert.Equal(sut, nextResult);
            }

            [Fact]
            public void Action_WhenErr_NextFunctionNotCalled()
            {
                /* Arrange */
                int called = 0;
                var sut = Result<string, _>.Error(1);
                Action<string> next = (result) => ++called;

                /* Act */
                var nextResult = sut.ThenRun(next);

                /* Assert */
                Assert.Equal(0, called);
            }

            [Fact]
            public void Action_WhenErr_SelfReturned()
            {
                /* Arrange */
                var sut = Result<string, _>.Error(1);

                /* Act */
                var nextResult = sut.ThenRun(A.Dummy<Action<string>>());

                /* Assert */
                Assert.Equal(sut, nextResult);
            }
        }

        public class ImplicitCastToValueKind : ResultOfTETests
        {
            [Fact]
            public void Error_ErrorKindReturned()
            {
                /* Arrange */
                var sut = Result<_, IFormattable>.Error(A.Dummy<IFormattable>());

                /* Act */
                ValueKind result = sut;

                /* Assert */
                Assert.Equal(ValueKind.Error, result);
            }

            [Fact]
            public void Ok_OkKindReturned()
            {
                /* Arrange */
                var sut = Result<string, _>.Ok(A.Dummy<string>());

                /* Act */
                ValueKind result = sut;

                /* Assert */
                Assert.Equal(ValueKind.Ok, result);
            }
        }

        public class ImplicitCastToBool : ResultOfTETests
        {
            [Fact]
            public void Error_FalseReturned()
            {
                /* Arrange */
                var sut = Result<_, IFormattable>.Error(A.Dummy<IFormattable>());

                /* Act */
                bool result = sut;

                /* Assert */
                Assert.False(result);
            }

            [Fact]
            public void Ok_TrueReturned()
            {
                /* Arrange */
                var sut = Result<string, _>.Ok(A.Dummy<string>());

                /* Act */
                bool result = sut;

                /* Assert */
                Assert.True(result);
            }
        }

        public class ExplicitCastToError : ResultOfTETests
        {
            [Fact]
            public void ResultIsError_ValueReturned()
            {
                /* Arrange */
                FormattableString value = $"foo";

                /* Act */
                var sut = Result<_, IFormattable>.Error(value);

                /* Assert */
                Assert.Same(value, (FormattableString)sut);
            }

            [Fact]
            public void ResultIsValue_BadStateExceptionThrown()
            {
                /* Arrange */
                var sut = Result<string, _>.Ok("foo");

                /* Act */
                var ex = Record.Exception(() => (_)sut);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "Cannot get the error because it is a valid value: foo",
                    ex.Message);
            }
        }

        public class OrElse : ResultOfTETests
        {
            [Fact]
            public void ResultIsError_ResultOfFuncReturned()
            {
                /* Arrange */
                FormattableString value = $"foo";
                FormattableString another = $"bar";
                var sut = Result<_,FormattableString>.Error(value);
                var expectedResult = Result<_,FormattableString>.Error(another);
                Func<FormattableString, Result<_, FormattableString>> onError =
                    e => expectedResult;

                /* Act */
                var orElseResult = sut.OrElse(onError);

                /* Assert */
                Assert.Equal(expectedResult, orElseResult);
            }

            [Fact]
            public void ResultIsOk_SameResultRetruned()
            {
                /* Arrange */
                var sut = Result<string,_>.Ok("foo");

                /* Act */
                var orElseResult = sut.OrElse(A.Dummy<Func<_, Result<string, _>>>());

                /* Assert */
                Assert.Equal(sut, orElseResult);
            }
        }

        public class Map : ResultOfTETests
        {
            [Fact]
            public void ResultIsError_ErrorKept()
            {
                /* Arrange */
                FormattableString value = $"foo";
                var sut = Result<_,FormattableString>.Error(value);
                Func<_, string> onOk = e => "bar";

                /* Act */
                var orElseResult = sut.Map(onOk);

                /* Assert */
                Assert.Same(value, orElseResult.UnwrapError());
            }

            [Fact]
            public void ResultIsOk_NewValueMapped()
            {
                /* Arrange */
                var sut = Result<int,FormattableString>.Ok(1);
                Func<int, string> onOk = e => "bar";

                /* Act */
                var orElseResult = sut.Map(onOk);

                /* Assert */
                Assert.Equal("bar", orElseResult.Unwrap());
            }
        }

        public class MapOrElse : ResultOfTETests
        {
            [Fact]
            public void ResultIsError_ErrorFunctionCalled()
            {
                /* Arrange */
                FormattableString value = $"bar";
                var sut = Result<_,FormattableString>.Error(value);
                Func<FormattableString, string> onError = e => $"foo {e}";

                /* Act */
                var mapOrElseResult = sut.MapOrElse(onError, A.Dummy<Func<_, string>>());

                /* Assert */
                Assert.Equal("foo bar", mapOrElseResult);
            }

            [Fact]
            public void ResultIsOk_OkFunctionCalled()
            {
                /* Arrange */
                FormattableString value = $"foo";
                var sut = Result<int,FormattableString>.Ok(1);
                Func<int, int> onOk = e => e + 2;

                /* Act */
                var mapOrElseResult = sut.MapOrElse(A.Dummy<Func<FormattableString, int>>(), onOk);

                /* Assert */
                Assert.Equal(3, mapOrElseResult);
            }
        }

        public class Into : ResultOfTETests
        {
            [Fact]
            public void OkValue_OkResultReturned()
            {
                /* Arrange */
                IFormattable okVal = $"foobar";
                var sut = Result<int,FormattableString>.Ok(1);

                /* Act */
                var result = sut.Into();

                /* Assert */
                Assert.Equal(Result.Ok, result);
            }

            [Fact]
            public void ErrorValue_InnerErrorReturned()
            {
                /* Arrange */
                var sut = Result<int,int>.Error(1);

                /* Act */
                var result = sut.Into();

                /* Assert */
                Assert.Equal(Result.Error(1), result);
            }
        }

        public class IEquatableEquals : ResultOfTETests
        {
            [Theory]
            [InlineData(1, 1, true)]
            [InlineData(2, 1, false)]
            public void BothOkResults_AreEqualWhenValuesEqual(int a, int b, bool expectEquals)
            {
                /* Arrange */
                var sut = Result<int,int>.Ok(a);
                var other = Result<int,int>.Ok(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }

            [Fact]
            public void ResultsHaveDifferentKind_AreNotEqual()
            {
                /* Arrange */
                var sut = Result<int,int>.Ok(1);
                var other = Result<int,int>.Error(2);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [MemberData(nameof(TestData.IFormattableArgs), MemberType = typeof(TestData))]
            public void BothErrorResults_AreEqualWhenValuesEqual(IFormattable a, IFormattable b, bool expectEquals)
            {
                /* Arrange */
                var sut = Result<int,IFormattable>.Error(a);
                var other = Result<int,IFormattable>.Error(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }
        }

        public class ObjectOverrideEquals : ResultOfTETests
        {
            [Fact]
            public void NotAResultArgument_AreEqual()
            {
                /* Arrange */
                var sut = Result<int,int>.Ok(1);

                /* Act */
                bool equal = sut.Equals("foo");

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [InlineData(1, 1, true)]
            [InlineData(2, 1, false)]
            public void BothOkResults_AreEqualWhenValuesEqual(int a, int b, bool expectEquals)
            {
                /* Arrange */
                var sut = Result<int,int>.Ok(a);
                object other = Result<int,int>.Ok(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }

            [Fact]
            public void ResultsHaveDifferentKind_AreNotEqual()
            {
                /* Arrange */
                var sut = Result<int,int>.Ok(1);
                object other = Result<int,int>.Error(2);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [MemberData(nameof(TestData.IFormattableArgs), MemberType = typeof(TestData))]
            public void BothErrorResults_AreEqualWhenValuesEqual(IFormattable a, IFormattable b, bool expectEquals)
            {
                /* Arrange */
                var sut = Result<int,IFormattable>.Error(a);
                object other = Result<int,IFormattable>.Error(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }
        }

        public class CompareTo : ResultOfTETests
        {
            [Theory]
            [InlineData(1, 1, 0)]
            [InlineData(1, 2, -1)]
            [InlineData(2, 1, 1)]
            public void BothOkResults_InnerValueCompared(int a, int b, int expectedOrder)
            {
                /* Arrange */
                var sut = Result<int,int>.Ok(a);
                var other = Result<int,int>.Ok(b);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(expectedOrder, order);
            }

            [Fact]
            public void ArgumentResultIsErrorAndInstanceIsOk_NegativeOneReturnedj()
            {
                /* Arrange */
                var sut = Result<int,int>.Ok(1);
                var other = Result<int,int>.Error(1);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(-1, order);
            }

            [Fact]
            public void ArgumentResultIsOkAndInstanceIsError_OneReturned()
            {
                /* Arrange */
                var sut = Result<int,int>.Error(1);
                var other = Result<int,int>.Ok(1);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(1, order);
            }

            [Theory]
            [InlineData(1, 1, 0)]
            [InlineData(1, 2, -1)]
            [InlineData(2, 1, 1)]
            public void BothResultsAreErrorKind_InnerValueCompared(int a, int b,
                int expectedOrder)
            {
                /* Arrange */
                var sut = Result<int,int>.Error(a);
                var other = Result<int,int>.Error(b);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(order, expectedOrder);
            }
        }

        public class ImplicitCastToResult : ResultOfTETests
        {
            [Fact]
            public void WhenOk_OkResultReturned()
            {
                /* Arrange */
                var sut = Result<int, _>.Ok(1);

                /* Act */
                Result cast = sut;

                /* Assert */
                Assert.Equal(Result.Ok, cast);
            }

            [Fact]
            public void WhenError_ErrorResultReturned()
            {
                /* Arrange */
                var error = A.Fake<IFormattable>();
                A.CallTo(() => error.ToString(null, null)).Returns("Foo");
                var sut = Result<int, IFormattable>.Error(error);

                /* Act */
                Result cast = sut;

                /* Assert */
                Assert.Equal(Result.Error($"Foo"), cast);
            }
        }

        public class ImplicitCastToResultOfT : ResultOfTETests
        {
            [Fact]
            public void WhenOk_OkResultOfTReturned()
            {
                /* Arrange */
                var sut = Result<int, _>.Ok(1);

                /* Act */
                Result<int> cast = sut;

                /* Assert */
                Assert.Equal(Result<int>.Ok(1), cast);
            }

            [Fact]
            public void WhenError_ErrorResultOfTReturned()
            {
                /* Arrange */
                var error = A.Fake<IFormattable>();
                A.CallTo(() => error.ToString(null, null)).Returns("Foo");
                var sut = Result<int, IFormattable>.Error(error);

                /* Act */
                Result<int> cast = sut;

                /* Assert */
                Assert.Equal(Result<int>.Error($"Foo"), cast);
            }
        }

        public class Or : ResultOfTETests
        {
            [Fact]
            public void WhenOk_ReturnsSelf()
            {
                /* Arrange */
                var sut = Result<string, _>.Ok("foo");
                var other = Result<string, _>.Ok("bar");

                /* Act */
                var nextResult = sut.Or(other);

                /* Assert */
                Assert.Equal(sut, nextResult);
            }

            [Fact]
            public void WhenErr_ReturnsOther()
            {
                /* Arrange */
                var sut = Result<string, _>.Error(1);
                var other = Result<string, _>.Ok("bar");

                /* Act */
                var nextResult = sut.Or(other);

                /* Assert */
                Assert.Equal(other, nextResult);
            }
        }

        private sealed class InvalidFooException : Exception {  }

        private sealed class ValidFooException : Exception
        {
            public ValidFooException(string msg): base(msg)
            {
            }
        }
    }
}
