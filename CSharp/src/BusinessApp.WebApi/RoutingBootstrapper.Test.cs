using Microsoft.AspNetCore.Builder;
using SimpleInjector;
using System.Collections.Generic;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Creates test routes
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
