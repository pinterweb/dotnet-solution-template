namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    using Xunit;
    using BusinessApp.WebApi.ProblemDetails;
    using System.Collections.Generic;
    using FakeItEasy;
    using System;
    using System.Linq;
    using BusinessApp.Domain;
    using BusinessApp.App;

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
                    ProblemType = typeof(ProblemTypeExceptionStub),
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
            public void NullArgument_DefaultInternalErrorStatusReturned()
            {
                /* Arrange */
                Exception unknown = null;

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal(500, problem.StatusCode);
            }

            [Fact]
            public void NullArgument_InternalErrorDetailReturnedWhenToStringIsNull()
            {
                /* Arrange */
                Exception unknown = null;

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal(
                    "An unknown error has occurred. Please try again or contact support",
                    problem.Detail
                );
            }

            [Fact]
            public void NullArgument_AboutBlankTypeReturned()
            {
                /* Arrange */
                Exception unknown = null;

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal("about:blank", problem.Type.ToString());
            }

            [Fact]
            public void UnknownExceptionType_InternalErrorStatusReturned()
            {
                /* Arrange */
                var unknown = A.Dummy<Exception>();

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal(500, problem.StatusCode);
            }

            [Fact]
            public void UnknownExceptionType_InternalErrorDetailReturnedWhenToStringIsNull()
            {
                /* Arrange */
                var unknown = new Exception("foobar");

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal("foobar", problem.Detail);
            }

            [Fact]
            public void UnknownExceptionType_AboutBlankTypeReturned()
            {
                /* Arrange */
                var unknown = A.Dummy<Exception>();

                /* Act */
                var problem = sut.Create(unknown);

                /* Assert */
                Assert.Equal("about:blank", problem.Type.ToString());
            }

            [Fact]
            public void KnownExceptionType_OptionStatusReturned()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal(400, problem.StatusCode);
            }

            [Fact]
            public void KnownExceptionType_OptionTypeReturned()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("http://bar/foo.html", problem.Type.ToString());
            }

            [Fact]
            public void KnownExceptionType_UsesOptionMessageOverride()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();
                options.First().MessageOverride = "lorem";

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("lorem", problem.Detail);
            }

            [Fact]
            public void KnownExceptionTypeNoMessageOverride_UsesErrorMessage()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub("msg from exception");
                options.First().MessageOverride = null;

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("msg from exception", problem.Detail);
            }

            [Fact]
            public void ExceptionHasData_ExtensionsAdded()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();
                error.Data.Add("foo", "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("bar", problem["foo"]);
            }

            [Theory]
            [InlineData("")]
            public void ExceptionWithoutDataKeyString_ExtensionsNotAdded(object key)
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();
                error.Data.Add(key, "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.False(problem.TryGetValue(key.ToString(), out object _));
            }

            [Fact]
            public void BatchException_CompositeProblemReturned()
            {
                /* Arrange */
                var results = new[]
                {
                    Result.OK,
                    Result.Error(A.Dummy<Exception>())
                };
                var error = BatchException.FromResults(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.IsType<CompositeProblemDetail>(problem);
            }

            [Fact]
            public void BatchException_ManyStatusesReturned()
            {
                /* Arrange */
                var results = new[]
                {
                    Result.OK,
                    Result.Error(A.Dummy<Exception>())
                };
                var error = BatchException.FromResults(results);

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
            public void BatchException_ManyTypesReturned()
            {
                /* Arrange */
                var results = new[]
                {
                    Result.OK,
                    Result.Error(new ProblemTypeExceptionStub())
                };
                var error = BatchException.FromResults(results);

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
            public void BatchException_ManyMessagesReturned()
            {
                /* Arrange */
                var results = new[]
                {
                    Result.OK,
                    Result.Error(new ProblemTypeExceptionStub("msg from exception"))
                };
                var error = BatchException.FromResults(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                var problems = Assert.IsType<CompositeProblemDetail>(problem);
                Assert.Collection<ProblemDetail>(problems,
                    p => Assert.Null(p.Detail),
                    p => Assert.Equal("msg from exception", p.Detail)
                );
            }

            [Fact]
            public void BatchException_ExceptionExtensionsAdded()
            {
                /* Arrange */
                var innerError = new ProblemTypeExceptionStub();
                innerError.Data.Add("foo", "bar");
                var results = new[]
                {
                    Result.OK,
                    Result.Error(innerError)
                };
                var error = BatchException.FromResults(results);

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
            public void BatchExceptionIsAMixOfExceptionWithoutStringKey_ExtensionsNotAdded(
                object key)
            {
                /* Arrange */
                var innerError = new ProblemTypeExceptionStub();
                innerError.Data.Add(key, "bar");
                var results = new[]
                {
                    Result.OK,
                    Result.Error(innerError)
                };
                var error = BatchException.FromResults(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.False(problem.TryGetValue(key.ToString(), out object _));
            }

            [Fact]
            public void BatchException_DetailMessagedAdded()
            {
                /* Arrange */
                var results = new[]
                {
                    Result.OK,
                    Result.Error(A.Dummy<ProblemTypeExceptionStub>())
                };
                var error = BatchException.FromResults(results);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal(
                    "The request partially succeeded. Please review the errors before continuing",
                    problem.Detail);
            }
        }

        private sealed class ProblemTypeExceptionStub : Exception
        {
            public ProblemTypeExceptionStub(string msg = null) : base(msg)
            {}
        }
    }
}
