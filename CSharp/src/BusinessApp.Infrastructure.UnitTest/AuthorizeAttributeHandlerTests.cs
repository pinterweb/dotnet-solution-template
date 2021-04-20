using System;
using System.Collections.Generic;
using System.Security.Principal;
using BusinessApp.Kernel;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class AuthorizeAttributeHandlerTests
    {
        private readonly AuthorizeAttributeHandler<CommandStub> sut;
        private readonly IPrincipal user;

        public AuthorizeAttributeHandlerTests()
        {
            user = A.Fake<IPrincipal>();

            sut = new AuthorizeAttributeHandler<CommandStub>(user);
        }

        public class Constructor : AuthorizeAttributeHandlerTests
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
            public void InvalidCtorArgs_ExceptionThrown(IPrincipal p)
            {
                /* Arrange */
                Action create = () => new AuthorizeAttributeHandler<CommandStub>(p);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BusinessAppException>(exception);
            }
        }

        public class AuthorizeObject : AuthorizeAttributeHandlerTests
        {
            [Fact]
            public void NoAuthorizeAttribute_TrueReturned()
            {
                /* Arrange */
                var nonAuthCommand = new CommandStub();

                /* Act */
                var authorized = sut.AuthorizeObject(nonAuthCommand);

                /* Assert */
                Assert.True(authorized);
            }

            [Fact]
            public void NoAuthorizeAttribute_UserRoleNotChecked()
            {
                /* Arrange */
                var nonAuthCommand = new CommandStub();

                /* Act */
                var authorized = sut.AuthorizeObject(nonAuthCommand);

                /* Assert */
                A.CallTo(() => user.IsInRole(A<string>._)).MustNotHaveHappened();
            }

            [Fact]
            public void HasAuthorizeAttribute_WithoutRoles_TrueReturned()
            {
                /* Arrange */
                var authCommand = new AuthCommandStub();

                /* Act */
                var authorized = sut.AuthorizeObject(authCommand);

                /* Assert */
                Assert.True(authorized);
            }

            [Fact]
            public void HasAuthorizeAttribute_WithoutRoles_UserRolesNotChecked()
            {
                /* Arrange */
                var authCommand = new AuthCommandStub();

                /* Act */
                var authorized = sut.AuthorizeObject(authCommand);

                /* Assert */
                A.CallTo(() => user.IsInRole(A<string>._)).MustNotHaveHappened();
            }

            [Fact]
            public void HasAuthorizeAttributeWithRoles_WhenInRole_TrueReturned()
            {
                /* Arrange */
                var authCommand = new AuthRolesCommandStub();
                A.CallTo(() => user.IsInRole(A<string>._)).Returns(true);

                /* Act */
                var authorized = sut.AuthorizeObject(authCommand);

                /* Assert */
                Assert.True(authorized);
            }

            [Fact]
            public void HasAuthorizeAttributeWithRoles_WhenNotInRole_FalseReturned()
            {
                /* Arrage */
                var authCommand = new AuthRolesCommandStub();
                var roles = new List<string>();
                A.CallTo(() => user.IsInRole(A<string>._))
                    .Invokes(c => roles.Add(c.GetArgument<string>(0)))
                    .Returns(false);

                /* Act */
                var _ = sut.AuthorizeObject(authCommand);

                /* Assert */
                Assert.Collection(roles,
                    r => Assert.Equal("Foo", r),
                    r => Assert.Equal("Bar", r));
            }

            [Fact]
            public void NotAuthorized_ReturnsFalse()
            {
                /* Arrage */
                var authCommand = new AuthRolesCommandStub();
                A.CallTo(() => user.IsInRole(A<string>._)).Returns(false);

                /* Act */
                var authorized = sut.AuthorizeObject(authCommand);

                /* Assert */
                Assert.False(authorized);
            }
        }
    }
}
