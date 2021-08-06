using System;
using System.Collections.Generic;
using System.Linq;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using BusinessApp.WebApi.ProblemDetails;
using FakeItEasy;
using Xunit;

namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    public class ProblemDetailFactoryTests
    {
        private readonly HashSet<ProblemDetailOptions> options;
        public readonly ProblemDetailFactory sut;

        public ProblemDetailFactoryTests()
        {
            options = new HashSet<ProblemDetailOptions>
            {
                new ProblemDetailOptions(typeof(ProblemTypeExceptionStub), 400)
                {
                    AbsoluteType = "http://bar/foo.html"
                },
                new ProblemDetailOptions(typeof(AnotherProblemTypeExceptionStub), 500)
                {
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
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class Create : ProblemDetailFactoryTests
        {
            [Fact]
            public void NullArgument_ExceptionThrown()
            {
                /* Arrange */
                Exception unknown = null;

                /* Act */
                var ex = Record.Exception(() => sut.Create(unknown));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
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

            [Fact]
            public void StatusNot404_WhenDataKeyToStringReturnsEmpty_GenericErrorKeyUsed()
            {
                /* Arrange */
                var error = new AnotherProblemTypeExceptionStub();
                error.Data.Add("", "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("bar", problem["Errors"]);
            }

            [Fact]
            public void StatusNot404_WhenDataKeyToStringReturnsNull_GenericErrorKeyUsed()
            {
                /* Arrange */
                var error = new AnotherProblemTypeExceptionStub();
                error.Data.Add(new DictionaryKey(), "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("bar", problem["Errors"]);
            }

            [Fact]
            public void Status404_WhenDataKeyToStringReturnsEmpty_GenericErrorKeyUsed()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();
                error.Data.Add("", "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("bar", problem["ValidationErrors"]);
            }

            [Fact]
            public void Status404_WhenDataKeyToStringReturnsNull_GenericErrorKeyUsed()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();
                error.Data.Add(new DictionaryKey(), "bar");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("bar", problem["ValidationErrors"]);
            }

            [Fact]
            public void Exception_WhenDataKeyExists_FirstTaken()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();
                error.Data.Add(new FirstDictionaryKey(), "bar");
                error.Data.Add(new SecondDictionaryKey(), "lorem");

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("bar", problem["A"]);
            }

            [Fact]
            public void ExceptionWithoutDataValueString_BlankValueAdded()
            {
                /* Arrange */
                var error = new ProblemTypeExceptionStub();
                error.Data.Add("bar", null);

                /* Act */
                var problem = sut.Create(error);

                /* Assert */
                Assert.Equal("", problem["bar"]);
            }

        }

        private sealed class ProblemTypeExceptionStub : Exception
        {
            public ProblemTypeExceptionStub(string msg = null) : base(msg)
            {}
        }

        private sealed class AnotherProblemTypeExceptionStub : Exception
        {
            public AnotherProblemTypeExceptionStub(string msg = null) : base(msg)
            {}
        }

        private sealed class DictionaryKey
        {
            public override string ToString() => null;
        }

        private sealed class AnotherDictionaryKey
        {
            public override string ToString() => null;
        }

        private sealed class FirstDictionaryKey
        {
            public override string ToString() => "A";
        }

        private sealed class SecondDictionaryKey
        {
            public override string ToString() => "A";
        }
    }
}
