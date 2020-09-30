namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    using Xunit;
    using BusinessApp.WebApi.ProblemDetails;
    using FakeItEasy;
    using System;

    public class ProblemDetailTests
    {
        public readonly ProblemDetail sut;

        public class Constructor : ProblemDetailTests
        {
            [Fact]
            public void StatusArg_StatusPropertySet()
            {
                /* Act */
                var sut = new ProblemDetail(200);

                /* Assert */
                Assert.Equal(200, sut.StatusCode);
            }

            [Fact]
            public void StatusArg_StatusSetInDictionary()
            {
                /* Act */
                var sut = new ProblemDetail(200);

                /* Assert */
                Assert.Equal(200, sut[nameof(ProblemDetail.StatusCode)]);
            }

            [Fact]
            public void NoTypeArg_AboutBlankSetOnTypeProperty()
            {
                /* Act */
                var sut = new ProblemDetail(A.Dummy<int>(), null);

                /* Assert */
                Assert.Equal("about:blank", sut.Type.ToString());
            }

            [Fact]
            public void NoTypeArg_AboutBlankSetOnTypeInDictionary()
            {
                /* Act */
                var sut = new ProblemDetail(A.Dummy<int>(), null);

                /* Assert */
                Assert.Equal("about:blank", sut[nameof(ProblemDetail.Type)].ToString());
            }

            [Fact]
            public void TitleProperty_SetFromStatusCode()
            {
                /* Act */
                var sut = new ProblemDetail(200);

                /* Assert */
                Assert.Equal("OK", sut.Title);
            }

            [Fact]
            public void TitleProperty_SetFromUnknownStatusCode()
            {
                /* Act */
                var sut = new ProblemDetail(50);

                /* Assert */
                Assert.Equal("Unknown status: 50", sut.Title);
            }

            [Fact]
            public void DetailPropertySetter_DetailValueSet()
            {
                /* Act */
                var sut = new ProblemDetail(A.Dummy<int>())
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
                var sut = new ProblemDetail(A.Dummy<int>())
                {
                    Detail = "foo"
                };

                /* Assert */
                Assert.Equal("foo", sut[nameof(ProblemDetail.Detail)]);
            }

            [Fact]
            public void InstancePropertySetter_InstanceValueSet()
            {
                /* Act */
                var sut = new ProblemDetail(A.Dummy<int>())
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
                var sut = new ProblemDetail(A.Dummy<int>())
                {
                    Instance = new Uri("/foo/bar", UriKind.Relative)
                };

                /* Assert */
                Assert.Equal("/foo/bar", sut[nameof(ProblemDetail.Instance)].ToString());
            }
        }

        public class IDictionaryImpl : ProblemDetailTests
        {
            [Fact]
            public void Count_HasRequiredProperties()
            {
                /* Act */
                var sut = new ProblemDetail(A.Dummy<int>());

                /* Assert */
                Assert.Equal(3, sut.Count);
            }

            [Fact]
            public void Keys_HasRequiredProperties()
            {
                /* Act */
                var sut = new ProblemDetail(A.Dummy<int>());

                /* Assert */
                Assert.Collection(sut.Keys,
                    key => Assert.Equal(key, nameof(ProblemDetail.StatusCode)),
                    key => Assert.Equal(key, nameof(ProblemDetail.Title)),
                    key => Assert.Equal(key, nameof(ProblemDetail.Type))
                );
            }

            [Fact]
            public void Enumeration_HasAllInCollection()
            {
                /* Act */
                var sut = new ProblemDetail(A.Dummy<int>());

                /* Assert */
                Assert.Collection(sut,
                    kvp => Assert.Equal(kvp.Key, nameof(ProblemDetail.StatusCode)),
                    kvp => Assert.Equal(kvp.Key, nameof(ProblemDetail.Title)),
                    kvp => Assert.Equal(kvp.Key, nameof(ProblemDetail.Type))
                );
            }
        }
    }
}
