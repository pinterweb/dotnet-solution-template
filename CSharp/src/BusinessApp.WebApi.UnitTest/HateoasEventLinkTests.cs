namespace BusinessApp.WebApi.UnitTest
{
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using Xunit;

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
                Assert.IsType<BadStateException>(exception);
            }

            [Fact]
            public void RelativeLinkFactory_CastsToEvent()
            {
                /* Arrange */
                HateoasLink<IDomainEvent> link =  new EventLinkStub("next");
                var e = new EventStub() { Id = "foobar" };

                /* Act */
                var linkTxt = link.RelativeLinkFactory(e);

                /* Assert */
                Assert.Equal("foobar", linkTxt);
            }

            private class EventLinkStub : HateoasEventLink<EventStub>
            {
                public EventLinkStub(string rel)
                    : base(rel)
                {}

                protected override Func<EventStub, string> EventRelativeLinkFactory { get; }
                    = e => e.Id.ToString();
            }
        }
    }
}
