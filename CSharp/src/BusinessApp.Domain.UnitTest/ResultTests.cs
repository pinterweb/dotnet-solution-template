namespace ShelLife.Domain.UnitTest
{
    using Xunit;
    using BusinessApp.Domain;
    using FakeItEasy;
    using System;

    public class ResultTests
    {
        public class OkFactory : ResultTests
        {
            [Fact]
            public void KindProperty_IsOk()
            {
                /* Arrange */
                string value = A.Dummy<string>();

                /* Act */
                var sut = Result<string, _>.Ok(A.Dummy<string>());

                /* Assert */
                Assert.Equal(Result.Ok, sut.Kind);
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
                var sut = Result<_, IFormattable>.Error(value);

                /* Assert */
                Assert.Equal(Result.Error, sut.Kind);
            }
        }

        public class ImplicitCastToValue
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
                    "Cannot implictly get the value because it is an error: foo",
                    ex.Message);
            }
        }

        public class Expect
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

        public class ExpectError
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

        public class UnwrapError
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

        public class Unwrap
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

        public class AndThen
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

        public class ImplicitCastToResult : ResultTests
        {
            [Fact]
            public void Error_ErrorKindReturned()
            {
                /* Arrange */
                var sut = Result<_, IFormattable>.Error(A.Dummy<IFormattable>());

                /* Act */
                Result result = sut;

                /* Assert */
                Assert.Equal(Result.Error, result);
            }

            [Fact]
            public void Ok_OkKindReturned()
            {
                /* Arrange */
                var sut = Result<string, _>.Ok(A.Dummy<string>());

                /* Act */
                Result result = sut;

                /* Assert */
                Assert.Equal(Result.Ok, result);
            }
        }

        public class ImplicitCastToBool : ResultTests
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

        public class ImplicitCastToError
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
                    "Cannot implictly get the error because it is a valid value: foo",
                    ex.Message);
            }
        }

        public class OrElse : ResultTests
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

        public class Map : ResultTests
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

        public class MapOrElse : ResultTests
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

        private sealed class InvalidFooException : Exception {  }
        private sealed class ValidFooException : Exception
        {
            public ValidFooException(string msg): base(msg)
            {
            }
        }
    }
}
