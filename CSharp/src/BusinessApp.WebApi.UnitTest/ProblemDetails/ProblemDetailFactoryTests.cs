namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    using Xunit;
    using BusinessApp.WebApi.ProblemDetails;
    using System.Collections.Generic;
    using FakeItEasy;
    using System;
    using System.Linq;
    using BusinessApp.Domain;
    using System.Collections;

    public class ProblemDetailFactoryTests
    {
        private readonly HashSet<ProblemDetailOptions> options;
        public readonly ProblemDetailFactory sut;

        public ProblemDetailFactoryTests()
        {
            options = new HashSet<ProblemDetailOptions>
            {
                new ProblemDetailOptions
                {
                    ProblemType = typeof(ProblemTypeStub),
                    StatusCode = 400,
                    AbsoluteType = "http://bar/foo.html"
                }
            };
            sut = new ProblemDetailFactory(options);
        }

        public class Constructor : ProblemDetailFactoryTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(HashSet<ProblemDetailOptions> o)
            {
                /* Arrange */
                void shouldThrow() => new ProblemDetailFactory(o);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }
        }

        public class Create : ProblemDetailFactoryTests
        {
            [Fact]
            public void UnknownArgumentType_InternalErrorStatusReturned()
            {
                /* Arrange */
                var unknown = A.Dummy<IFormattable>();

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal(500, problem.StatusCode);
            }

            [Fact]
            public void UnknownArgumentType_InternalErrorDetailReturned()
            {
                /* Arrange */
                var unknown = A.Dummy<IFormattable>();

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal(
                     "An unknown error has occurred. Please try again or " +
                    "contact support",
                    problem.Detail
                );
            }

            [Fact]
            public void UnknownArgumentType_AboutBlankTypeReturned()
            {
                /* Arrange */
                var unknown = A.Dummy<IFormattable>();

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal("about:blank", problem.Type.ToString());
            }

            [Fact]
            public void KnownArgumentType_OptionStatusReturned()
            {
                /* Arrange */
                var error = new ProblemTypeStub();

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal(400, problem.StatusCode);
            }

            [Fact]
            public void KnownArgumentType_OptionTypeReturned()
            {
                /* Arrange */
                var error = new ProblemTypeStub();

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("http://bar/foo.html", problem.Type.ToString());
            }

            [Fact]
            public void KnownArgumentType_UsesOptionMessageOverride()
            {
                /* Arrange */
                var error = new ProblemTypeStub();
                options.First().MessageOverride = "lorem";

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("lorem", problem.Detail);
            }

            [Fact]
            public void KnownArgumentTypeNoMessageOverride_UsesErrorMessage()
            {
                /* Arrange */
                var error = new ProblemTypeStub();
                options.First().MessageOverride = null;

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("msg from formattable", problem.Detail);
            }

            [Fact]
            public void ArgumentTypeIsExceptionWithStringKey_ExtensionsAdded()
            {
                /* Arrange */
                var error = new FormattableExceptionStub();
                error.Data.Add("foo", "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("bar", problem["foo"]);
            }

            [Fact]
            public void ArgumentTypeIsExceptionIFormattableKey_ExtensionsAdded()
            {
                /* Arrange */
                var key = A.Fake<IFormattable>();
                A.CallTo(() => key.ToString("g", null)).Returns("foo");
                var error = new FormattableExceptionStub();
                error.Data.Add(key, "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("bar", problem["foo"]);
            }

            [Theory]
            [InlineData("")]
            public void ArgumentTypeIsExceptionWithoutKeyString_ExtensionsNotAdded(object key)
            {
                /* Arrange */
                var error = new FormattableExceptionStub();
                error.Data.Add(key, "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.False(problem.TryGetValue(key.ToString(), out object _));
            }

            [Fact]
            public void ArgumentTypeACompositeOfAllOkResults_ExceptionThrown()
            {
                /* Arrange */
                var results = new Result<_, IFormattable>[]
                {
                    Result<_, IFormattable>.Ok(new _())
                };
                var error = new CompositeError(results);

                /* Act */
                var ex = Record.Exception(() => sut.Create(error));

                /* Assert */
                Assert.IsType<BusinessAppWebApiException>(ex);
                Assert.Equal(
                    "A multi status problem should have at least one error",
                    ex.Message
                );
            }

            [Fact]
            public void ArgumentTypeIsAMixOfErrors_CompositeProblemReturned()
            {
                /* Arrange */
                var results = new Result<_, IFormattable>[]
                {
                    Result<_, IFormattable>.Ok(new _()),
                    Result<_, IFormattable>.Error(1)
                };
                var error = new CompositeError(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.IsType<CompositeProblemDetail>(problem);
            }

            [Fact]
            public void ArgumentTypeIsAMixOfErrors_ManyStatusesReturned()
            {
                /* Arrange */
                var results = new Result<_, IFormattable>[]
                {
                    Result<_, IFormattable>.Ok(new _()),
                    Result<_, IFormattable>.Error(1)
                };
                var error = new CompositeError(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                var problems = Assert.IsType<CompositeProblemDetail>(problem);
                Assert.Collection<ProblemDetail>(problems,
                    p => Assert.Equal(200, p.StatusCode),
                    p => Assert.Equal(500, p.StatusCode)
                );
            }

            [Fact]
            public void ArgumentTypeIsAMixOfErrors_ManyTypesReturned()
            {
                /* Arrange */
                var results = new Result<_, IFormattable>[]
                {
                    Result<_, IFormattable>.Ok(new _()),
                    Result<_, IFormattable>.Error(new ProblemTypeStub())
                };
                var error = new CompositeError(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                var problems = Assert.IsType<CompositeProblemDetail>(problem);
                Assert.Collection<ProblemDetail>(problems,
                    p => Assert.Equal("about:blank", p.Type.ToString()),
                    p => Assert.Equal("http://bar/foo.html", p.Type.ToString())
                );
            }

            [Fact]
            public void ArgumentTypeIsAMixOfErrors_ManyMessagesReturned()
            {
                /* Arrange */
                var results = new Result<_, IFormattable>[]
                {
                    Result<_, IFormattable>.Ok(new _()),
                    Result<_, IFormattable>.Error(new ProblemTypeStub())
                };
                var error = new CompositeError(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                var problems = Assert.IsType<CompositeProblemDetail>(problem);
                Assert.Collection<ProblemDetail>(problems,
                    p => Assert.Null(p.Detail),
                    p => Assert.Equal("msg from formattable", p.Detail)
                );
            }

            [Fact]
            public void ArgumentTypeIsAMixOfErrors_ExceptionExtensionsAdded()
            {
                /* Arrange */
                var innerError = new FormattableExceptionStub();
                innerError.Data.Add("foo", "bar");
                var results = new Result<_, IFormattable>[]
                {
                    Result<_, IFormattable>.Ok(new _()),
                    Result<_, IFormattable>.Error(innerError)
                };
                var error = new CompositeError(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                var problems = Assert.IsType<CompositeProblemDetail>(problem);
                Assert.Collection<ProblemDetail>(problems,
                    p => Assert.False(p.TryGetValue("foo", out object _)),
                    p => Assert.Equal("bar", p["foo"])
                );
            }

            [Fact]
            public void ArgumentTypeIsAMixOfErrorsWithIFormattableKey_ExceptionExtensionsAdded()
            {
                /* Arrange */
                var key = A.Fake<IFormattable>();
                var innerError = new FormattableExceptionStub();
                A.CallTo(() => key.ToString("g", null)).Returns("foo");
                innerError.Data.Add(key, "bar");
                var results = new Result<_, IFormattable>[]
                {
                    Result<_, IFormattable>.Ok(new _()),
                    Result<_, IFormattable>.Error(innerError)
                };
                var error = new CompositeError(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                var problems = Assert.IsType<CompositeProblemDetail>(problem);
                Assert.Collection<ProblemDetail>(problems,
                    p => Assert.False(p.TryGetValue("foo", out object _)),
                    p => Assert.Equal("bar", p["foo"])
                );
            }

            [Theory]
            [InlineData(1)]
            [InlineData("")]
            public void ArgumentTypeIsAMixOfExceptionWithoutStringKey_ExtensionsNotAdded(object key)
            {
                /* Arrange */
                var innerError = new FormattableExceptionStub();
                innerError.Data.Add(key, "bar");
                var results = new Result<_, IFormattable>[]
                {
                    Result<_, IFormattable>.Ok(new _()),
                    Result<_, IFormattable>.Error(innerError)
                };
                var error = new CompositeError(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.False(problem.TryGetValue(key.ToString(), out object _));
            }
        }

        private sealed class ProblemTypeStub : IFormattable
        {
            public string ToString(string format, IFormatProvider formatProvider)
            {
                return "msg from formattable";
            }
        }

        private sealed class FormattableExceptionStub : Exception, IFormattable
        {
            public string ToString(string format, IFormatProvider formatProvider)
            {
                return "";
            }
        }

        private sealed class CompositeError : IFormattable, IEnumerable<Result<_, IFormattable>>
        {
            private readonly IEnumerable<Result<_, IFormattable>> results;

            public CompositeError(IEnumerable<Result<_, IFormattable>> results)
            {
                this.results = results;
            }

            public IEnumerator<Result<_, IFormattable>> GetEnumerator() => results.GetEnumerator();

            public string ToString(string format, IFormatProvider formatProvider)
            {
                return "";
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}
