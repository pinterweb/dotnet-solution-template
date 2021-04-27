using System.Security.Principal;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// <see cref="IPrincipal" /> implementation for an anonymous user
    /// </summary>
    public class AnonymousUser : IPrincipal
    {
        public const string Name = "Anonymous";

        private readonly IPrincipal inner;

        public AnonymousUser(IPrincipal inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            Identity = new AnonymousIdentity(inner.Identity);
        }

        public IIdentity Identity { get; }

        public bool IsInRole(string role) => inner.IsInRole(role);

        private class AnonymousIdentity : IIdentity
        {
            private readonly IIdentity? inner;

            public AnonymousIdentity(IIdentity? inner) => this.inner = inner;

            public string? AuthenticationType => inner?.AuthenticationType;
            public bool IsAuthenticated => inner?.IsAuthenticated ?? false;
            public string Name => !IsAuthenticated
                ? AnonymousUser.Name
                : (inner?.Name ?? AnonymousUser.Name);
        }
    }
}
