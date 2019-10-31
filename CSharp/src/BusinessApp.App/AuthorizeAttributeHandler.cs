namespace BusinessApp.App
{
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Principal;
    using BusinessApp.Domain;

    /// <summary>
    /// Base class to authorizes a user in the context of {T} based on the <see cref="AuthorizeAttribute" />
    /// </summary>
    public abstract class AuthorizeAttributeHandler<T>
    {
        protected static readonly AuthorizeAttribute Attribute =
            typeof(T).GetTypeInfo().GetCustomAttribute<AuthorizeAttribute>();

        private readonly IPrincipal currentUser;
        private readonly ILogger logger;

        public AuthorizeAttributeHandler(IPrincipal currentUser, ILogger logger)
        {
            this.currentUser = currentUser;
            this.logger = logger;
        }

        protected void Authorize(string executionContext)
        {
            var allowedRoleCount = Attribute.Roles.Count();

            for (int i = allowedRoleCount - 1; i >= 0; i--)
            {
                if (currentUser.IsInRole(Attribute.Roles.ElementAt(i))) break;

                if (i == 0)
                {
                    logger.Log(
                        new LogEntry(
                            LogSeverity.Info,
                            $"User {currentUser.Identity.Name} is not authorized to execute " +
                            executionContext
                        )
                    );

                    throw new SecurityException();
                }
            }
        }
    }
}
