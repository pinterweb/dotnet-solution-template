#if winauth
using Microsoft.AspNetCore.Authorization;
#endif
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Creates your routes
    /// </summary>
    public static partial class RoutingBootstrapper
    {
        public static void SetupEndpoints(this IApplicationBuilder app,
            Func<IEndpointRouteBuilder, IEnumerable<IEndpointConventionBuilder>> routeBuilder)
            => app.UseEndpoints(endpoint =>
            {
                var endpoints = routeBuilder(endpoint);
#if winauth
                foreach (var ep in endpoints)
                {
                    ep.RequireAuthorization(new AuthorizeAttribute());
                }
#endif
            });
    }

}
