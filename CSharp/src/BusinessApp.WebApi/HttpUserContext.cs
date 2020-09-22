namespace BusinessApp.WebApi
{
    using System.Security.Principal;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.Domain;

    /// <summary>
    /// Http user implementation of the IPrincipal
    /// </summary>
    public class HttpUserContext : IPrincipal
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpUserContext(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = Guard.Against.Null(httpContextAccessor).Expect(nameof(httpContextAccessor));
        }

        public IIdentity Identity => Principal.Identity;
        public bool IsInRole(string role) => Principal.IsInRole(role);
        private IPrincipal Principal => httpContextAccessor.HttpContext.User;
    }
}
