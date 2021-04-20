using System;
using System.Collections.Generic;
using System.Security.Principal;
using BusinessApp.Domain;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class AnonymousUserTests
    {
        private readonly IPrincipal inner;

        public AnonymousUserTests()
        {
            inner = A.Fake<IPrincipal>();
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
                Assert.IsType<BusinessAppException>(exception);
            }
        }

        public class IdentityProperty : AnonymousUserTests
        {
            [Fact]
            public void NullIdentity_NullAuthenticationType()
            {
                /* Arrange */
                A.CallTo(() => inner.Identity).Returns(null);
                var sut = new AnonymousUser(inner);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Null(identity.AuthenticationType);
            }

            [Fact]
            public void NullIdentity_NotAuthenticated()
            {
                /* Arrange */
                A.CallTo(() => inner.Identity).Returns(null);
                var sut = new AnonymousUser(inner);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.False(identity.IsAuthenticated);
            }

            [Fact]
            public void NullIdentity_AnonymousName()
            {
                /* Arrange */
                A.CallTo(() => inner.Identity).Returns(null);
                var sut = new AnonymousUser(inner);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Equal("Anonymous", identity.Name);
            }

            [Fact]
            public void NullIdentityName_AnonymousName()
            {
                /* Arrange */
                A.CallTo(() => inner.Identity.Name).Returns(null);
                var sut = new AnonymousUser(inner);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Equal("Anonymous", identity.Name);
            }

            [Fact]
            public void AuthenticationType_InnerValueReturned()
            {
                /* Arrange */
                A.CallTo(() => inner.Identity.AuthenticationType).Returns("foo");
                var sut = new AnonymousUser(inner);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Equal("foo", identity.AuthenticationType);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void IsAuthentication_InnerValueReturned(bool authenticated)
            {
                /* Arrange */
                A.CallTo(() => inner.Identity.IsAuthenticated).Returns(authenticated);
                var sut = new AnonymousUser(inner);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Equal(authenticated, identity.IsAuthenticated);
            }

            [Theory]
            [InlineData(false, "Anonymous")]
            [InlineData(true, "")]
            public void Name_WhenNameIsEmpty_AnonymousReturned(bool authenticated, string expectedName)
            {
                /* Arrange */
                A.CallTo(() => inner.Identity.IsAuthenticated).Returns(authenticated);
                var sut = new AnonymousUser(inner);

                /* Act */
                var identity = sut.Identity;

                /* Assert */
                Assert.Equal(expectedName, identity.Name);
            }
        }
    }
}
