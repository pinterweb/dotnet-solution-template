#if winauth
using Microsoft.AspNetCore.Authorization;
#endif
using System;
using System.Collections.Generic;
using BusinessApp.Infrastructure.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Creates your routes
    /// </summary>
    public static class Routes
    {
        public static IEnumerable<IEndpointConventionBuilder> CreateRoutes(this IEndpointRouteBuilder builder,
            Container container)
        {
            var endpoints = System.Array.Empty<IEndpointConventionBuilder>();

            // TODO create routes here
            // IHttpRequestHandler getHandler() => container.GetInstance<IHttpRequestHandler>();

            // builder.MapGet("/api/resources", getHandler().HandleAsync<Get.Request, IEnumerable<Get.Response>>);

            return endpoints;
        }
    }
}
