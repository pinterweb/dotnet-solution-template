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
                    return new []
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
                Assert.IsType<BadStateException>(exception);
            }
        }
    }
}
