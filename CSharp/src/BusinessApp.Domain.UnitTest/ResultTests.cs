namespace ShelLife.Domain.UnitTest
{
    using Xunit;
    using BusinessApp.Domain;
    using FakeItEasy;

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
                var sut = Result<string>.Ok(value);

                /* Assert */
                Assert.Equal(Result.Ok, sut.Kind);
            }

            [Fact]
            public void CastToValue_ValueReturned()
            {
                /* Arrange */
                string value = "foo";

                /* Act */
                var sut = Result<string>.Ok(value);

                /* Assert */
                Assert.Same(value, (string)sut);
            }

            [Fact]
            public void Expect_ValueReturned()
            {
                /* Arrange */
                string value = "foo";

                /* Act */
                var sut = Result<string>.Ok(value);

                /* Assert */
                Assert.Same(value, sut.Expect(A.Dummy<string>()));
            }
        }

        public class FailureFactory : ResultTests
        {
            [Fact]
            public void KindProperty_IsFailure()
            {
                /* Arrange */
                string value = A.Dummy<string>();

                /* Act */
                var sut = Result<string>.Failure(value);

                /* Assert */
                Assert.Equal(Result.Failure, sut.Kind);
            }

            [Fact]
            public void CastToValue_BadStateExceptionThrown()
            {
                /* Arrange */
                string value = "foo";
                var sut = Result<string>.Failure(value);

                /* Act */
                var ex = Record.Exception(() => (string)sut);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "Cannot get the value because it is invalid in the current context",
                    ex.Message);
            }

            [Fact]
            public void Expect_BadStateExceptionThrown()
            {
                /* Arrange */
                string value = "foo";
                var sut = Result<string>.Failure(value);

                /* Act */
                var ex = Record.Exception(() => sut.Expect("Some message"));

                /* Assert */
                Assert.IsType<BadStateException>(ex);
                Assert.Equal(
                    "Some message",
                    ex.Message);
            }
        }

        public class CastToResult : ResultTests
        {
            [Fact]
            public void Failure_FailureKindReturned()
            {
                /* Arrange */
                var sut = Result<string>.Failure(A.Dummy<string>());

                /* Act */
                Result result = sut;

                /* Assert */
                Assert.Equal(Result.Failure, result);
            }

            [Fact]
            public void Ok_OkKindReturned()
            {
                /* Arrange */
                var sut = Result<string>.Ok(A.Dummy<string>());

                /* Act */
                Result result = sut;

                /* Assert */
                Assert.Equal(Result.Ok, result);
            }
        }

        public class CastToBool : ResultTests
        {
            [Fact]
            public void Failure_FalseReturned()
            {
                /* Arrange */
                var sut = Result<string>.Failure(A.Dummy<string>());

                /* Act */
                bool result = sut;

                /* Assert */
                Assert.False(result);
            }

            [Fact]
            public void Ok_TrueReturned()
            {
                /* Arrange */
                var sut = Result<string>.Ok(A.Dummy<string>());

                /* Act */
                bool result = sut;

                /* Assert */
                Assert.True(result);
            }
        }
    }
}
