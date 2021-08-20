using System;
using System.Collections.Generic;
using BusinessApp.Kernel;
using BusinessApp.Test.Shared;
using FakeItEasy;
using Xunit;

namespace BusinessApp.WebApi.UnitTest
{
    public class HateoasEventLinkTests
    {
        public class Constructor : HateoasEventLinkTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[] { null },
                        new object[] { "" },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidArgs_ExceptionThrown(string r)
            {
                /* Arrange */
                Action create = () => new EventLinkStub(r);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BusinessAppException>(exception);
            }

            [Fact]
            public void RelativeLinkFactory_CastsToEvent()
            {
                /* Arrange */
                HateoasLink<RequestStub, IDomainEvent> link =  new EventLinkStub("next");
                var e = new EventStub() { Id = "foobar" };

                /* Act */
                var linkTxt = link.RelativeLinkFactory(A.Dummy<RequestStub>(), e);

                /* Assert */
                Assert.Equal("foobar", linkTxt);
            }

            private class EventLinkStub : HateoasEventLink<RequestStub, EventStub>
            {
                public EventLinkStub(string rel)
                    : base(rel)
                {}

                protected override Func<RequestStub, EventStub, string> EventRelativeLinkFactory { get; }
                    = (r, e) => e.Id.ToString();
            }
        }
    }
}
