using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Kernel;
#if winauth
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Builder;
using SimpleInjector;

//#if DEBUG
using System.Collections.Generic;
using System.Linq;
//#endif

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Creates your routes
    /// </summary>
    public static partial class RoutingBootstrapper
    {
        public static void SetupTestEndpoints(this IApplicationBuilder app, Container container)
            => app.UseEndpoints(endpoint =>
            {
                // make a func so we can get it in the http request scope
                // and use other scope services
                IHttpRequestHandler getHandler() => container.GetInstance<IHttpRequestHandler>();

                var endpoints = new IEndpointConventionBuilder[]
                {
                    endpoint.MapGet("/api/resources",
                        getHandler().HandleAsync<Get.Request, IEnumerable<Get.Response>>),

                    endpoint.MapGet("/api/resources/{id:int}",
                        getHandler().HandleAsync<Get.Request, Get.Response>),

                    endpoint.MapPost("/api/resources",
                        getHandler().HandleAsync<PostOrPut.Body, PostOrPut.Body>),

                    endpoint.MapPut("/api/resources/{id:int}",
                        getHandler().HandleAsync<PostOrPut.Body, PostOrPut.Body>),

                    endpoint.MapDelete("/api/resources/{id:int}",
                        getHandler().HandleAsync<Delete.Query, Delete.Response>),
                };
            });
    }

}
