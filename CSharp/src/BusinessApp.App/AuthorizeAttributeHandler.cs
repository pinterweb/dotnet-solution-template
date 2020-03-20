namespace BusinessApp.App
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Principal;
    using BusinessApp.Domain;

    /// <summary>
    /// Implementation to authorizes a user in the context of {T} based on the <see cref="AuthorizeAttribute" />
    /// </summary>
    public class AuthorizeAttributeHandler<T> : IAuthorizer<T>
    {
        private static AuthorizeAttribute Attribute = GetAttribute(typeof(T));

        private readonly IPrincipal currentUser;
        private readonly ILogger logger;

        public AuthorizeAttributeHandler(IPrincipal currentUser, ILogger logger)
        {
            this.currentUser = GuardAgainst.Null(currentUser, nameof(logger));
            this.logger = GuardAgainst.Null(logger, nameof(logger));
        }

        public void AuthorizeObject(T instance)
        {
            if (Attribute == null)
            {
                // for child classes of {T}
                Attribute = GetAttribute(instance.GetType());

                if (Attribute != null) AuthorizeObject(instance);
            }
            else
            {
                var allowedRoleCount = Attribute.Roles.Count();

                for (int i = allowedRoleCount - 1; i >= 0; i--)
                {
                    if (currentUser.IsInRole(Attribute.Roles.ElementAt(i))) break;

                    if (i == 0)
                    {
                        var msgTemplate = $"{{0}} not authorized to execute {instance.GetType().Name}";
                        var ex= new SecurityException(string.Format(msgTemplate, "You are"));

                        logger.Log(
                            new LogEntry(
                                LogSeverity.Info,
                                string.Format(msgTemplate, $"User '{currentUser.Identity.Name}' is"),
                                ex
                            )
                        );

                        throw ex;
                    }
                }
            }
        }

        private static AuthorizeAttribute GetAttribute(Type commandType)
        {
            return commandType.GetTypeInfo().GetCustomAttribute<AuthorizeAttribute>();
        }
    }
}
