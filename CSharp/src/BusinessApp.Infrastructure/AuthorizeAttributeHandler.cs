using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Implementation to authorizes a user in the context of {T} based on the <see cref="AuthorizeAttribute" />
    /// </summary>
    public class AuthorizeAttributeHandler<T> : IAuthorizer<T>
        where T : notnull
    {
        private AuthorizeAttribute? authAttribute = GetAttribute(typeof(T));

        private readonly IPrincipal currentUser;

        public AuthorizeAttributeHandler(IPrincipal currentUser)
            => this.currentUser = currentUser.NotNull().Expect(nameof(currentUser));

        public bool AuthorizeObject(T instance)
        {
            if (authAttribute == null)
            {
                // for child classes of {T}
                authAttribute = GetAttribute(instance.GetType());

                if (authAttribute != null) return AuthorizeObject(instance);
            }
            else
            {
                var allowedRoleCount = authAttribute.Roles.Count();

                return allowedRoleCount == 0
                    || authAttribute.Roles.Any(a => currentUser.IsInRole(a));
            }

            return true;
        }

        private static AuthorizeAttribute? GetAttribute(Type commandType)
            => commandType.GetTypeInfo().GetCustomAttribute<AuthorizeAttribute>();
    }
}
