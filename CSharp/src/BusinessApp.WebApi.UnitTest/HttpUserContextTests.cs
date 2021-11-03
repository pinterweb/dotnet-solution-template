using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using BusinessApp.Kernel;

namespace BusinessApp.WebApi.UnitTest
{
    public class HttpUserContextTests
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly HttpContext context;
        private readonly HttpUserContext sut;

        public HttpUserContextTests()
        {
            contextAccessor = A.Fake<IHttpContextAccessor>();
            sut = new HttpUserContext(contextAccessor);
            context = A.Fake<HttpContext>();
            A.CallTo(() => contextAccessor.HttpContext).Returns(context);
        }

        public class Constructor : HttpUserContextTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null }
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IHttpContextAccessor a)
            {
                /* Arrange */
                void shouldThrow() => new HttpUserContext(a);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class IdentityProperty : HttpUserContextTests
        {
            [Fact]
            public void ReturnsPrincipalIdentity()
            {
                /* Arrange */
                var principal = A.Fake<ClaimsPrincipal>();
                var identity = A.Fake<IIdentity>();
                A.CallTo(() => context.User).Returns(principal);
                A.CallTo(() => principal.Identity).Returns(identity);

                /* Act */
                var userIdentity = sut.Identity;

                /* Assert */
                Assert.Same(identity, userIdentity);
            }
        }

        public class IsIntRole : HttpUserContextTests
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void DelegatesToInnerPrincipal(bool isInRole)
            {
                /* Arrange */
                A.CallTo(() => context.User.IsInRole("foo")).Returns(isInRole);

                /* Act */
                var actualIsInRole = sut.IsInRole("foo");

                /* Assert */
                Assert.Equal(isInRole, actualIsInRole);
            }
        }
    }
}
