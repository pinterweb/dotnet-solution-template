namespace BusinessApp.WebApi.UnitTest
{
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class HateoasLinkTests
    {
        public class Constructor : HateoasLinkTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new[]
                    {
                        new object[]
                        {
                            null,
                            "rel"
                        },
                        new object[]
                        {
                            A.Dummy<Func<RequestStub, QueryStub, string>>(),
                            null
                        },
                        new object[]
                        {
                            A.Dummy<Func<RequestStub, QueryStub, string>>(),
                            ""
                        },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidArgs_ExceptionThrown(Func<RequestStub, QueryStub, string> f,
                string r)
            {
                /* Arrange */
                Action create = () => new HateoasLink<RequestStub, QueryStub>(f, r);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BusinessAppException>(exception);
            }

            [Fact]
            public void RelArgOnlyInChildClass_SetsRelLink()
            {
                /* Act */
                var child = new ChildHateoasLinkStub("foo");

                /* Assert */
                Assert.Equal("foo", child.RelativeLinkFactory(1, 2));
            }
        }

        private sealed class ChildHateoasLinkStub : HateoasLink<int, int>
        {
            public ChildHateoasLinkStub(string rel) : base(rel)
            { }
        }
    }
}
