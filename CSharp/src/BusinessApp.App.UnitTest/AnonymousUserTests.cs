namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class AnonymousUserTests
    {
        private readonly AnonymousUser sut;
        private readonly IPrincipal inner;

        public AnonymousUserTests()
        {
            inner = A.Fake<IPrincipal>();

            sut = new AnonymousUser(inner);
        }

        public class Constructor : AnonymousUserTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[] { null },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void WithAnyNullService_ExceptionThrown(IPrincipal u)
            {
                /* Arrange */
                Action create = () => new AnonymousUser(u);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BadStateException>(exception);
            }
        }


        public class IdentityProperty : AnonymousUserTests
        {
            [Fact]
            public void AuthenticationType_InnerIdentityValueReturned()
            {
                /* Arrange */
                A.CallTo(() => inner.Identity.AuthenticationType).Returns("foo");

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Equal("foo", identity.AuthenticationType);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void IsAuthentication_InnerIdentityValueReturned(bool authenticated)
            {
                /* Arrange */
                A.CallTo(() => inner.Identity.IsAuthenticated).Returns(authenticated);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Equal(authenticated, identity.IsAuthenticated);
            }

            [Theory]
            [InlineData(false, "Anonymous")]
            [InlineData(true, "foo")]
            public void Name_AnonymousReturnedIfNameIsEmpty(bool authenticated, string expectedName)
            {
                /* Arrange */
                A.CallTo(() => inner.Identity.IsAuthenticated).Returns(authenticated);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Equal(expectedName, identity.Name);
            }
        }
    }
}
