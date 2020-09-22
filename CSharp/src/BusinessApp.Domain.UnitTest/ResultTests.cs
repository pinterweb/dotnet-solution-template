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
            public void CastToValue_ValueReturned()
            {
                /* Arrange */
                string value = "foo";

                /* Act */
                var sut = Result<string, _>.Ok(value);

                /* Assert */
                Assert.Same(value, (string)sut);
            }

            [Fact]
            public void CastToValue_BadStateExceptionThrown()
            {
                /* Arrange */
                IFormattable value = $"foo";
                var sut = Result<_, IFormattable>.Error(value);

                /* Act */
                var ex = Record.Exception(() => (_)sut);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "Cannot get the results value because it is in an error state.",
                    ex.Message);
            }
        }

        public class Expect
        {
            [Fact]
            public void Expect_BadStateExceptionThrown()
            {
                /* Arrange */
                IFormattable value = $"foo";
                var sut = Result<_, IFormattable>.Error(value);

                /* Act */
                var ex = Record.Exception(() => sut.Expect("Some message"));

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "Some message: foo",
                    ex.Message);
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

        private sealed class InvalidFooException : Exception {  }
        private sealed class ValidFooException : Exception
        {
            public ValidFooException(string msg): base(msg)
            {
            }
        }
    }
}
