using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Http user implementation of the IPrincipal
    /// </summary>
    public class HttpUserContext : IPrincipal
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpUserContext(IHttpContextAccessor httpContextAccessor)
            => this.httpContextAccessor = httpContextAccessor.NotNull().Expect(nameof(httpContextAccessor));

        public IIdentity? Identity => Principal?.Identity;
        public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
        private IPrincipal? Principal => httpContextAccessor.HttpContext?.User;
    }
}
