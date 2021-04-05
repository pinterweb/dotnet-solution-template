namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Security.Principal;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class AuthorizeAttributeHandlerTests
    {
        private readonly AuthorizeAttributeHandler<CommandStub> sut;
        private readonly IPrincipal user;
        private readonly ILogger logger;

        public AuthorizeAttributeHandlerTests()
        {
            user = A.Fake<IPrincipal>();
            logger = A.Fake<ILogger>();

            sut = new AuthorizeAttributeHandler<CommandStub>(user, logger);
        }

        public class Constructor : AuthorizeAttributeHandlerTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[] { null, A.Dummy<ILogger>() },
                        new object[] { A.Dummy<IPrincipal>(), null }
                    };
                }
            }


            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IPrincipal p, ILogger l)
            {
                /* Arrange */
                Action create = () => new AuthorizeAttributeHandler<CommandStub>(p , l);

                /* Act */
                var exception = Record.Exception(create);

                /* Assert */
                Assert.IsType<BadStateException>(exception);
            }
        }

        public class AuthorizeObject : AuthorizeAttributeHandlerTests
        {
            [Fact]
            public void NoAuthorizeAttribute_ExceptionNotThrown()
            {
                /* Arrange */
                var nonAuthCommand = new CommandStub();
                Action validate = () => sut.AuthorizeObject(nonAuthCommand);

                /* Act */
                var exception = Record.Exception(validate);

                /* Assert */
                Assert.Null(exception);
                A.CallTo(() => user.IsInRole(A<string>._)).MustNotHaveHappened();
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }

            [Fact]
            public void HasAuthorizeAttributeWithoutRoles_DoesNothing()
            {
                /* Arrange */
                var authCommand = new AuthCommandStub();
                Action validate = () => sut.AuthorizeObject(authCommand);

                /* Act */
                var exception = Record.Exception(validate);

                /* Assert */
                Assert.Null(exception);
                A.CallTo(() => user.IsInRole(A<string>._)).MustNotHaveHappened();
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }

            [Fact]
            public void HasAuthorizeAttributeWithRoles_DoesNothingWhenInAnyRole()
            {
                /* Arrange */
                var authCommand = new AuthRolesCommandStub();
                Action validate = () => sut.AuthorizeObject(authCommand);
                A.CallTo(() => user.IsInRole(A<string>._)).Returns(true);

                /* Act */
                var exception = Record.Exception(validate);

                /* Assert */
                Assert.Null(exception);
                A.CallTo(() => user.IsInRole(A<string>._)).MustHaveHappenedOnceExactly();
                A.CallTo(() => user.IsInRole("Bar")).MustHaveHappenedOnceExactly();
                A.CallTo(() => logger.Log(A<LogEntry>._)).MustNotHaveHappened();
            }

            public class NotAuthorized : AuthorizeObject
            {
                private readonly Action runAuthorization;
                private readonly IIdentity identity;

                public NotAuthorized()
                {
                    /* Arrange */
                    var authCommand = new AuthRolesCommandStub();
                    runAuthorization = () => sut.AuthorizeObject(authCommand);
                    identity = A.Fake<IIdentity>();

                    A.CallTo(() => user.Identity).Returns(identity);
                    A.CallTo(() => user.IsInRole(A<string>._)).Returns(false);
                }

                [Fact]
                public void ChecksAllRoles()
                {
                    /* Act */
                    var exception = Record.Exception(runAuthorization);

                    /* Assert */
                    A.CallTo(() => user.IsInRole(A<string>._)).MustHaveHappenedTwiceExactly();
                    A.CallTo(() => user.IsInRole("Foo")).MustHaveHappenedOnceExactly();
                    A.CallTo(() => user.IsInRole("Bar")).MustHaveHappenedOnceExactly();
                }

                [Fact]
                public void ThrowSecurityException()
                {
                    /* Act */
                    var exception = Record.Exception(runAuthorization);

                    /* Assert */
                    var securityEx = Assert.IsType<SecurityException>(exception);
                    Assert.Equal(
                        "You are not authorized to execute AuthRolesCommandStub",
                        securityEx.Message
                    );
                }

                [Theory]
                [InlineData(null, "Anonymous")]
                [InlineData("foouser", "foouser")]
                public void LogsException(string identityName, string usedName)
                {
                    /* Arrange */
                    LogEntry logEntry = null;
                    A.CallTo(() => logger.Log(A<LogEntry>._))
                        .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));
                    A.CallTo(() => identity.Name).Returns(identityName);

                    /* Act */
                    var exception = Record.Exception(runAuthorization);

                    /* Assert */
                    A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
                    Assert.Equal(LogSeverity.Info, logEntry.Severity);
                    Assert.Equal(
                        $"User '{usedName}' is not authorized to execute AuthRolesCommandStub",
                        logEntry.Message
                    );
                }

                [Fact]
                public void NullIdentity_LogsAnonymous()
                {
                    /* Arrange */
                    LogEntry logEntry = null;
                    A.CallTo(() => logger.Log(A<LogEntry>._))
                        .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));
                    A.CallTo(() => user.Identity).Returns(null);

                    /* Act */
                    var exception = Record.Exception(runAuthorization);

                    /* Assert */
                    A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
                    Assert.Equal(LogSeverity.Info, logEntry.Severity);
                    Assert.Equal(
                        $"User '{AnonymousUser.Name}' is not authorized to execute AuthRolesCommandStub",
                        logEntry.Message
                    );
                }
            }
        }
    }
}
