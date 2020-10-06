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
                static BatchException create() => new BatchException(null);

                /* Act */
                var ex = Record.Exception(create);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }

            [Fact]
            public void WithEmptyResults_ExceptionThrown()
            {
                /* Arrange */
                var results = A.Dummy<IEnumerable<Result>>();
                static BatchException create() => new BatchException(null);

                /* Act */
                var ex = Record.Exception(create);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }

            [Fact]
            public void WithResults_PropertySet()
            {
                /* Arrange */
                var results = A.CollectionOfDummy<Result>(1);

                /* Act */
                var sut = new BatchException(results);

                /* Assert */
                Assert.Single(sut.Results);
            }

            [Fact]
            public void NoInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerError = Result.Error($"foobar");
                var innerOk = Result.Ok;

                /* Act */
                var sut = new BatchException(new[] { innerError, innerOk });

                /* Assert */
                Assert.Collection<Result>(sut,
                    s => Assert.Equal(innerError, s),
                    s => Assert.Equal(innerOk, s)
                );
                Assert.Collection<Result<IFormattable, IFormattable>>(sut,
                    s => Assert.Equal(innerError.Into(), s),
                    s => Assert.Equal(innerOk.Into(), s)
                );
            }

            [Fact]
            public void HasInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerError = Result.Error($"foobar");
                var innerOk = Result.Ok;
                var results = Result.Error(new BatchException(new[] { innerError, innerOk }));

                /* Act */
                var sut = new BatchException(new[] { results });

                /* Assert */
                Assert.Collection<Result>(sut,
                    s => Assert.Equal(innerError, s),
                    s => Assert.Equal(innerOk, s)
                );
                Assert.Collection<Result<IFormattable, IFormattable>>(sut,
                    s => Assert.Equal(innerError.Into(), s),
                    s => Assert.Equal(innerOk.Into(), s)
                );
            }

            [Fact]
            public void HasDeepInnerBatchException_MaintainsResultOrder()
            {
                /* Arrange */
                var innerError = Result.Error($"foobar");
                var innerOk = Result.Ok;
                var innerSut = new BatchException(new[] { innerError, innerOk });
                var innerResults = Result.Error(innerSut);
                var outerError = Result.Error(innerSut);
                var outerOk = Result.Ok;
                var outerSut = new BatchException(new[] { outerError, outerOk });
                var outerResults = Result.Error(outerSut);

                /* Act */
                var sut = new BatchException(new[] { outerResults });

                /* Assert */
                Assert.Collection<Result>(sut,
                    s => Assert.Equal(innerError, s),
                    s => Assert.Equal(innerOk, s),
                    s => Assert.Equal(outerOk, s)
                );
                Assert.Collection<Result<IFormattable, IFormattable>>(sut,
                    s => Assert.Equal(innerError.Into(), s),
                    s => Assert.Equal(innerOk.Into(), s),
                    s => Assert.Equal(outerOk.Into(), s)
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
                    Result.Error(A.Dummy<IFormattable>()),
                    Result.Error(A.Dummy<IFormattable>()),
                    Result.Ok
                };

                /* Act */
                var sut = new BatchException(results);

                /* Assert */
                Assert.Equal("The batch request has 2 out of 3 errors", sut.ToString());
            }
        }
    }
}
