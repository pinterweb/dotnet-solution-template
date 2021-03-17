namespace BusinessApp.App
{
    using System.Security.Principal;
    using BusinessApp.Domain;

    /// <summary>
    /// Http user implementation of the IPrincipal
    /// </summary>
    public class AnonymousUser : IPrincipal
    {
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
            private readonly IIdentity inner;

            public AnonymousIdentity(IIdentity inner)
            {
                this.inner = inner.NotNull().Expect(nameof(inner));
            }

            public string AuthenticationType => inner.AuthenticationType;
            public bool IsAuthenticated => inner.IsAuthenticated;
            public string Name => !inner.IsAuthenticated ? "Anonymous" : inner.Name;
        }
    }
}
