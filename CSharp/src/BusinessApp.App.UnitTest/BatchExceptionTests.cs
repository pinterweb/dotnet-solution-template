namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using BusinessApp.Domain;
    using FakeItEasy;

    public class BatchExceptionTests
    {
        public class FromResults : BatchExceptionTests
        {
            [Fact]
            public void WithoutResults_ExceptionThrown()
            {
                /* Arrange */
                static BatchException create() => BatchException.FromResults<string>(null);

                /* Act */
                var ex = Record.Exception(create);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void WithEmptyResults_ExceptionThrown()
            {
                /* Arrange */
                var results = A.CollectionOfDummy<Result<string, Exception>>(0);
                BatchException create() => BatchException.FromResults(results);

                /* Act */
                var ex = Record.Exception(create);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void NoInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerException = new Exception("foobar");
                var innerError = Result.Error<string>(innerException);
                var innerOk = Result.Ok("lorem");

                /* Act */
                var sut = BatchException.FromResults(new[] { innerError, innerOk });

                /* Assert */
                Assert.Collection<Result<object, Exception>>(sut,
                    s => Assert.Equal(Result.Error<object>(innerException), s),
                    s => Assert.Equal(Result.Ok<object>("lorem"), s)
                );
            }

            [Fact]
            public void HasMultipleErrorMessage()
            {
                /* Arrange */
                var innerException = new Exception("foobar");
                var innerError = Result.Error<string>(innerException);

                /* Act */
                var sut = BatchException.FromResults(new[] { innerError });

                /* Assert */
                Assert.Equal("Multiple errors occurred. See messages for errors.", sut.Message);
            }

            [Fact]
            public void HasInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerException = new Exception("foobar");
                var innerError = Result.Error<string>(innerException);
                var innerOk = Result.Ok("lorem");
                var results = Result.Error(BatchException.FromResults(new[] { innerError, innerOk }));

                /* Act */
                var sut = BatchException.FromResults(new[] { results });

                /* Assert */
                Assert.Collection(sut,
                    s => Assert.Equal(Result.Error<object>(innerException), s),
                    s => Assert.Equal(Result.Ok<object>("lorem"), s)
                );
            }

            [Fact]
            public void HasDeepInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerException = new Exception("foobar");
                var innerError = Result.Error<string>(innerException);
                var innerOk = Result.Ok("lorem");
                var innerSut = BatchException.FromResults(new[] { innerError, innerOk });
                var innerResults = Result.Error(innerSut);
                var outerError = Result.Error<string>(innerSut);
                var outerOk = Result.Ok("ipsum");
                var outerSut = BatchException.FromResults(new[] { outerError, outerOk });
                var outerResults = Result.Error(outerSut);

                /* Act */
                var sut = BatchException.FromResults(new[] { outerResults });

                /* Assert */
                Assert.Collection<Result<object, Exception>>(sut,
                    s => Assert.Equal(Result.Error<object>(innerException), s),
                    s => Assert.Equal(Result.Ok<object>("lorem"), s),
                    s => Assert.Equal(Result.Ok<object>("ipsum"), s)
                );
            }
        }
    }
}
