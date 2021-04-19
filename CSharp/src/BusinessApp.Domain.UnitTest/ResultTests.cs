using Xunit;
using FakeItEasy;
using System;
using _ = System.Int32;

namespace BusinessApp.Domain.UnitTest
{
    public class ResultTests
    {
        public class OkValue : ResultTests
        {
            [Fact]
            public void NewUnitReturned()
            {
                /* Act */
                var result = Result.OK;
                /* Assert */
                Assert.Equal(Result<Unit, Exception>.Ok(Unit.New), result);
            }
        }

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
                Assert.Equal(Result<string, Exception>.Ok("foo"), result);
            }

            [Fact]
            public void FuncThrows_IsError()
            {
                /* Arrange */
                var error = A.Fake<Exception>();
                Func<string> noop = () => throw error;

                /* Act */
                var result = Result.From(noop);

                /* Assert */
                Assert.Equal(Result.Error<string>(error), result);
            }
        }

        public class OkFactory : ResultTests
        {
            [Fact]
            public void OkResultReturned()
            {
                /* Act */
                var sut = Result.Ok<string>("foo");

                /* Assert */
                Assert.Equal(Result<string, Exception>.Ok("foo"), sut);
            }
        }

        public class ErrorFactoryOfT : ResultTests
        {
            [Fact]
            public void ErrorResultReturned()
            {
                /* Act */
                var error = new Exception();
                var sut = Result.Error<string>(error);

                /* Assert */
                Assert.Equal(Result<string, Exception>.Error(error), sut);
            }
        }

        public class ErrorFactory : ResultTests
        {
            [Fact]
            public void ErrorResultReturned()
            {
                /* Act */
                var error = new Exception();
                var sut = Result.Error(error);

                /* Assert */
                Assert.Equal(Result<Unit, Exception>.Error(error), sut);
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
            public void NullError_Throws()
            {
                /* Arrange */
                string error = null;

                /* Act */
                var ex = Record.Exception(() => Result<_, string>.Error(error));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void KindProperty_IsError()
            {
                /* Arrange */
                var value = A.Dummy<Exception>();

                /* Act */
                var sut = Result.Error<string>(value);

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
                var value = "foo";
                var sut = Result<_, string>.Error(value);

                /* Act */
                var ex = Record.Exception(() => (_)sut);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
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
                var error = $"foo";
                var sut = Result<_, string>.Error(error);

                /* Act */
                var ex = Record.Exception(() => sut.Expect("Some message"));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal(
                    "Some message: foo",
                    ex.Message);
            }

            [Fact]
            public void IsOkResult_ValueReturned()
            {
                /* Arrange */
                var value = "foo";
                var sut = Result<string, _>.Ok(value);

                /* Act */
                var okVal = sut.Expect("Some message");

                /* Assert */
                Assert.Equal("foo", okVal);
            }
        }

        public class ExpectError : ResultOfTETests
        {
            [Fact]
            public void IsOkResult_BadStateExceptionThrown()
            {
                /* Arrange */
                var value = "foo";
                var sut = Result<string, _>.Ok(value);

                /* Act */
                var ex = Record.Exception(() => sut.ExpectError("Some message"));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal(
                    "Some message: foo",
                    ex.Message);
            }

            [Fact]
            public void IsErrorResult_ErrorReturned()
            {
                /* Arrange */
                var error = "foo";
                var sut = Result<_, string>.Error(error);

                /* Act */
                var errVal = sut.ExpectError(A.Dummy<string>());

                /* Assert */
                Assert.Equal("foo", errVal);
            }
        }

        public class UnwrapError : ResultOfTETests
        {
            [Fact]
            public void IsOkResult_BadStateExceptionThrown()
            {
                /* Arrange */
                var value = "foo";
                var sut = Result<string, _>.Ok(value);

                /* Act */
                var ex = Record.Exception(() => sut.UnwrapError());

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal(
                    "foo",
                    ex.Message);
            }

            [Fact]
            public void IsErrorResult_ErrorReturned()
            {
                /* Arrange */
                var error = "foo";
                var sut = Result<_, string>.Error(error);

                /* Act */
                var errVal = sut.UnwrapError();

                /* Assert */
                Assert.Equal("foo", errVal);
            }
        }

        public class Unwrap : ResultOfTETests
        {
            [Fact]
            public void IsErrorResult_BadStateExceptionThrown()
            {
                /* Arrange */
                var error = "foo";
                var sut = Result<_, string>.Error(error);

                /* Act */
                var ex = Record.Exception(() => sut.Unwrap());

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void IsOkResult_ValueReturned()
            {
                /* Arrange */
                var value = "foo";
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
                var expectResult = Result<bool, _>.Ok(true);
                Func<string, Result<bool, _>> next = (result) => expectResult;

                /* Act */
                var nextResult = sut.AndThen(next);

                /* Assert */
                Assert.Equal(expectResult, nextResult);
            }

            [Fact]
            public void WhenErr_SelfReturned()
            {
                /* Arrange */
                string value = "foo";
                var sut = Result<_, string>.Error(value);
                Func<_, Result<_, string>> next = (result) => A.Dummy<Result<_, string>>();

                /* Act */
                var nextResult = sut.AndThen(next);

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
                var sut = Result<_, string>.Error(A.Dummy<string>());

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
                var sut = Result<_, string>.Error(A.Dummy<string>());

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
                var value = "foo";

                /* Act */
                var sut = Result<_, string>.Error(value);

                /* Assert */
                Assert.Same(value, (string)sut);
            }

            [Fact]
            public void ResultIsValue_BadStateExceptionThrown()
            {
                /* Arrange */
                var sut = Result<string, _>.Ok("foo");

                /* Act */
                var ex = Record.Exception(() => (_)sut);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
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
                var value = "foo";
                var another = "bar";
                var sut = Result<_,string>.Error(value);
                var expectedResult = Result<_,string>.Error(another);
                Func<string, Result<_, string>> onError = e => expectedResult;

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
                var value = "foo";
                var sut = Result<_, string>.Error(value);
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
                var sut = Result<int, string>.Ok(1);
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
                var value = "bar";
                var sut = Result<_, string>.Error(value);
                Func<string, string> onError = e => $"foo {e}";

                /* Act */
                var mapOrElseResult = sut.MapOrElse(onError, A.Dummy<Func<_, string>>());

                /* Assert */
                Assert.Equal("foo bar", mapOrElseResult);
            }

            [Fact]
            public void ResultIsOk_OkFunctionCalled()
            {
                /* Arrange */
                var sut = Result<int, string>.Ok(1);
                Func<int, int> onOk = e => e + 2;

                /* Act */
                var mapOrElseResult = sut.MapOrElse(A.Dummy<Func<string, int>>(), onOk);

                /* Assert */
                Assert.Equal(3, mapOrElseResult);
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
                var sut = Result<int, _>.Ok(a);
                var other = Result<int, _>.Ok(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }

            [Fact]
            public void ResultsHaveDifferentKind_AreNotEqual()
            {
                /* Arrange */
                var sut = Result<int, _>.Ok(1);
                var other = Result<_, int>.Error(2);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void BothErrorResults_AreEqualWhenValuesEqual(bool expectEquals)
            {
                /* Arrange */
                var err1 = A.Fake<Exception>();
                var err2 = A.Fake<Exception>();
                A.CallTo(() => err1.Equals(err2)).Returns(expectEquals);
                var sut = Result<int, Exception>.Error(err1);
                var other = Result<int, Exception>.Error(err2);

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
                var sut = Result<int, _>.Ok(1);

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
                var sut = Result<int, _>.Ok(a);
                object other = Result<int, _>.Ok(b);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectEquals, equal);
            }

            [Fact]
            public void ResultsHaveDifferentKind_AreNotEqual()
            {
                /* Arrange */
                var sut = Result<int, _>.Ok(1);
                object other = Result<_, int>.Error(2);

                /* Act */
                bool equal = sut.Equals(other);

                /* Assert */
                Assert.False(equal);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void BothErrorResults_AreEqualWhenValuesEqual(bool expectEquals)
            {
                /* Arrange */
                var err1 = A.Fake<Exception>();
                var err2 = A.Fake<Exception>();
                A.CallTo(() => err1.Equals(err2)).Returns(expectEquals);
                var sut = Result<int, Exception>.Error(err1);
                object other = Result<int, Exception>.Error(err2);

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
                var sut = Result<int, _>.Ok(a);
                var other = Result<int, _>.Ok(b);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(expectedOrder, order);
            }

            [Fact]
            public void ArgumentResultIsErrorAndInstanceIsOk_NegativeOneReturnedj()
            {
                /* Arrange */
                var sut = Result<int, _>.Ok(1);
                var other = Result<_, int>.Error(1);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(-1, order);
            }

            [Fact]
            public void ArgumentResultIsOkAndInstanceIsError_OneReturned()
            {
                /* Arrange */
                var sut = Result<_, int>.Error(1);
                var other = Result<int, _>.Ok(1);

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
                var sut = Result<_, int>.Error(a);
                var other = Result<_, int>.Error(b);

                /* Act */
                var order = sut.CompareTo(other);

                /* Assert */
                Assert.Equal(order, expectedOrder);
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
    }
}
