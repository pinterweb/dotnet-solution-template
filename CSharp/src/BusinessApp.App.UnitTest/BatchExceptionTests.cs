namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using BusinessApp.Domain;
    using FakeItEasy;
    using System.Collections.Generic;

    public class BatchExceptionTests
    {
        public class Constructor : BatchExceptionTests
        {
            [Fact]
            public void WithoutResults_ExceptionThrown()
            {
                /* Arrange */
                BatchException create() => new BatchException(null);

                /* Act */
                var ex = Record.Exception(create);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }

            [Fact]
            public void WithEmptyResults_ExceptionThrown()
            {
                /* Arrange */
                var results = A.Dummy<IEnumerable<Result<_, IFormattable>>>();
                BatchException create() => new BatchException(null);

                /* Act */
                var ex = Record.Exception(create);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }

            [Fact]
            public void WithResults_PropertySet()
            {
                /* Arrange */
                var results = A.CollectionOfDummy<Result<_, IFormattable>>(1);

                /* Act */
                var sut = new BatchException(results);

                /* Assert */
                Assert.Single(sut.Results);
            }

            [Fact]
            public void NoInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerError = Result<_, IFormattable>.Error($"foobar");
                var innerOk = Result<_, IFormattable>.Ok(new _());

                /* Act */
                var sut = new BatchException(new[] { innerError, innerOk });

                /* Assert */
                Assert.Collection(sut,
                    s => Assert.Equal(innerError, s),
                    s => Assert.Equal(innerOk, s)
                );
            }

            [Fact]
            public void HasInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerError = Result<_, IFormattable>.Error($"foobar");
                var innerOk = Result<_, IFormattable>.Ok(new _());
                var results = Result<_, IFormattable>.Error(new BatchException(new[] { innerError, innerOk }));

                /* Act */
                var sut = new BatchException(new[] { results });

                /* Assert */
                Assert.Collection(sut,
                    s => Assert.Equal(innerError, s),
                    s => Assert.Equal(innerOk, s)
                );
            }

            [Fact]
            public void HasDeepInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerError = Result<_, IFormattable>.Error($"foobar");
                var innerOk = Result<_, IFormattable>.Ok(new _());
                var innerSut = new BatchException(new[] { innerError, innerOk });
                var innerResults = Result<_, IFormattable>.Error(innerSut);
                var outerError = Result<_, IFormattable>.Error(innerSut);
                var outerOk = Result<_, IFormattable>.Ok(new _());
                var outerSut = new BatchException(new[] { outerError, outerOk });
                var outerResults = Result<_, IFormattable>.Error(outerSut);

                /* Act */
                var sut = new BatchException(new[] { outerResults });

                /* Assert */
                Assert.Collection(sut,
                    s => Assert.Equal(innerError, s),
                    s => Assert.Equal(innerOk, s),
                    s => Assert.Equal(outerOk, s)
                );
            }
        }

        public class IFormattableImpl : BatchExceptionTests
        {
            [Fact]
            public void ToString_ReturnsFormattedMessage()
            {
                /* Arrange */
                var results = new[]
                {
                    Result<_, IFormattable>.Error(A.Dummy<IFormattable>()),
                    Result<_, IFormattable>.Error(A.Dummy<IFormattable>()),
                    Result<_, IFormattable>.Ok(new _())
                };

                /* Act */
                var sut = new BatchException(results);

                /* Assert */
                Assert.Equal("The batch request has 2 out of 3 errors", sut.ToString());
            }
        }
    }
}
