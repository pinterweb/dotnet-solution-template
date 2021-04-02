namespace BusinessApp.App
{
    using System.Security.Principal;
    using BusinessApp.Domain;

    /// <summary>
    /// <see cref="IPrincipal" /> implementation for an anonymous user
    /// </summary>
    public class AnonymousUser : IPrincipal
    {
        public const string Name = "Anonymous";

        private readonly IPrincipal inner;
        private readonly IIdentity identity;

        public AnonymousUser(IPrincipal inner)
        {
            this.inner = inner.NotNull().Expect(nameof(inner));
            identity = new AnonymousIdentity(inner.Identity);
        }

        public IIdentity Identity => identity;

        public bool IsInRole(string role) => inner.IsInRole(role);

        private class AnonymousIdentity : IIdentity
        {
            private readonly IIdentity? inner;

            public AnonymousIdentity(IIdentity? inner)
            {
                this.inner = inner;
            }

            public string? AuthenticationType => inner?.AuthenticationType;
            public bool IsAuthenticated => inner?.IsAuthenticated ?? false;
            public string Name => !IsAuthenticated
                ? AnonymousUser.Name
                : (inner?.Name ?? AnonymousUser.Name);
        }
    }
}
