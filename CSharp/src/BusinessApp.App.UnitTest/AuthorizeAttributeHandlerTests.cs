namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
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
        public void WhenCreating_ExceptionThrownIfAnyArgumentNull(IPrincipal p, ILogger l)
        {
            /* Arrange */
            Action create = () => new AuthorizeAttributeHandler<CommandStub>(p , l);

            /* Act */
            var exception = Record.Exception(create);

            /* Assert */
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void InstanceDoesNotHaveTheAuthorizeAttribute_ExceptionNotThrown()
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
        public void InstanceHasTheAuthorizeAttributeWithoutRoles_DoesNothing()
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
        public void InstanceHasTheAuthorizeAttributeWithRoles_DoesNothingWhenInAnyRole()
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

        [Fact]
        public void InstanceHasTheAuthorizeAttributeWithRoles_LogsAndThrowsWhenUserNotInAnyRole()
        {
            /* Arrange */
            var authCommand = new AuthRolesCommandStub();
            var fakeIdentity = A.Fake<IIdentity>();
            Action validate = () => sut.AuthorizeObject(authCommand);
            LogEntry logEntry = null;

            A.CallTo(() => fakeIdentity.Name).Returns("foouser");
            A.CallTo(() => user.Identity).Returns(fakeIdentity);
            A.CallTo(() => user.IsInRole(A<string>._)).Returns(false);
            A.CallTo(() => logger.Log(A<LogEntry>._))
                .Invokes(ctx => logEntry = ctx.GetArgument<LogEntry>(0));

            /* Act */
            var exception = Record.Exception(validate);

            /* Assert */
            Assert.NotNull(exception);
            A.CallTo(() => user.IsInRole(A<string>._)).MustHaveHappenedTwiceExactly();
            A.CallTo(() => user.IsInRole("Foo")).MustHaveHappenedOnceExactly();
            A.CallTo(() => user.IsInRole("Bar")).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Log(A<LogEntry>._)).MustHaveHappenedOnceExactly();
            Assert.Equal(LogSeverity.Info, logEntry.Severity);
            Assert.Equal(
                "User 'foouser' is not authorized to execute AuthRolesCommandStub",
                logEntry.Message
            );
        }
    }
}
