namespace BusinessApp.App
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using BusinessApp.Domain;

    /// <summary>
    /// Implementation to authorizes a user in the context of {T} based on the <see cref="AuthorizeAttribute" />
    /// </summary>
    public class AuthorizeAttributeHandler<T> : IAuthorizer<T>
        where T : notnull
    {
        private AuthorizeAttribute? Attribute = GetAttribute(typeof(T));

        private readonly IPrincipal currentUser;

        public AuthorizeAttributeHandler(IPrincipal currentUser)
        {
            this.currentUser = currentUser.NotNull().Expect(nameof(currentUser));
        }

        public bool AuthorizeObject(T instance)
        {
            if (Attribute == null)
            {
                // for child classes of {T}
                Attribute = GetAttribute(instance.GetType());

                if (Attribute != null) return AuthorizeObject(instance);
            }
            else
            {
                var allowedRoleCount = Attribute.Roles.Count();

                return allowedRoleCount == 0
                    ? true
                    : Attribute.Roles.Any(a => currentUser.IsInRole(a));
            }

            return true;
        }

        private static AuthorizeAttribute? GetAttribute(Type commandType)
        {
            return commandType.GetTypeInfo().GetCustomAttribute<AuthorizeAttribute>();
        }
    }
}
