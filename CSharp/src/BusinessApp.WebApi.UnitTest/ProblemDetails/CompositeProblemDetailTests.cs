namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    using Xunit;
    using BusinessApp.WebApi.ProblemDetails;
    using FakeItEasy;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessApp.Domain;

    public class CompositeProblemDetailTests
    {
        public readonly CompositeProblemDetail sut;
        public readonly IEnumerable<ProblemDetail> problems;

        public class Constructor : CompositeProblemDetailTests
        {
            [Fact]
            public void EmptyProblemsArg_ExceptionThrown()
            {
                /* Act */
                var ex = Record.Exception(() =>
                    new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(0)));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void ProblemsArg_ProblemsPropertySet()
            {
                /* Arrange */
                var problems = A.CollectionOfDummy<ProblemDetail>(1);

                /* Act */
                var sut = new CompositeProblemDetail(problems);

                /* Assert */
                Assert.Same(problems, sut.Responses);
            }

            [Fact]
            public void StatusProperty_MultiStatusValueSet()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1));

                /* Assert */
                Assert.Equal(207, sut.StatusCode);
            }

            [Fact]
            public void StatusArg_StatusSetInDictionary()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1));

                /* Assert */
                Assert.Equal(207, sut[nameof(CompositeProblemDetail.StatusCode)]);
            }

            [Fact]
            public void NoTypeArg_AboutBlankSetOnTypeProperty()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1));

                /* Assert */
                Assert.Equal("about:blank", sut.Type.ToString());
            }

            [Fact]
            public void NoTypeArg_AboutBlankSetOnTypeInDictionary()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1));

                /* Assert */
                Assert.Equal("about:blank", sut[nameof(CompositeProblemDetail.Type)].ToString());
            }

            [Fact]
            public void TitleProperty_SetFromStatusCode()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1));

                /* Assert */
                Assert.Equal("MultiStatus", sut.Title);
            }

            [Fact]
            public void DetailPropertySetter_DetailValueSet()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1))
                {
                    Detail = "foo"
                };

                /* Assert */
                Assert.Equal("foo", sut.Detail);
            }

            [Fact]
            public void DetailPropertySetter_DetailSetInDictionary()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1))
                {
                    Detail = "foo"
                };

                /* Assert */
                Assert.Equal("foo", sut[nameof(CompositeProblemDetail.Detail)]);
            }

            [Fact]
            public void InstancePropertySetter_InstanceValueSet()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1))
                {
                    Instance = new Uri("/foo/bar", UriKind.Relative)
                };

                /* Assert */
                Assert.Equal("/foo/bar", sut.Instance.ToString());
            }

            [Fact]
            public void InstancePropertySetter_InstanceSetInDictionary()
            {
                /* Act */
                var sut = new CompositeProblemDetail(A.CollectionOfDummy<ProblemDetail>(1))
                {
                    Instance = new Uri("/foo/bar", UriKind.Relative)
                };

                /* Assert */
                Assert.Equal("/foo/bar", sut[nameof(CompositeProblemDetail.Instance)].ToString());
            }
        }

        public class GetEnumeratorImpl : CompositeProblemDetailTests
        {
            [Fact]
            public void UsesInnerProblemsEnumerator()
            {
                /* Arrange */
                var problems = new[]
                {
                    A.Dummy<ProblemDetail>(),
                    A.Dummy<ProblemDetail>(),
                };

                /* Act */
                var sut = new CompositeProblemDetail(problems);

                /* Assert */
                Assert.Collection(problems,
                    p => Assert.Same(problems.First(), p),
                    p => Assert.Same(problems.Last(), p)
                );
            }
        }
    }
}
