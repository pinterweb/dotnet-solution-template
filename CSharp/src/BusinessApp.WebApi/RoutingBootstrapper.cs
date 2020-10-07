namespace BusinessApp.WebApi
{
#if winauth
    using Microsoft.AspNetCore.Authorization;
#endif
    using Microsoft.AspNetCore.Builder;
    using SimpleInjector;

    /// <summary>
    /// Creates your routes
    /// </summary>
    public static class RoutingBootstrapper
    {
        public static void SetupEndpoints(this IApplicationBuilder app, Container container)
        {
            app.UseRouting();

#if winauth
            app.UseAuthentication();
            app.UseAuthorization();
#endif

            #region TODO APIS HERE

            app.UseEndpoints(endpoint =>
            {
                var endpoints = new IEndpointConventionBuilder[]
                {
                    //endpoint.MapGet("/api/<resource>", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<SomeQuery, IEnumerable<BusinessContract>>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapGet("/api/<resource>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<SomeQuery, BusinessContract>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapPost("/api/<resource>", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<BusinessCommand, BusinessCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapPut("/api/<resource>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<BusinessCommand, BusinessCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapDelete("/api/<resource>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IHttpRequestHandler<DeleteCommand, DeleteCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                };

#if winauth
                foreach (var ep in endpoints)
                {
                    ep.RequireAuthorization(new AuthorizeAttribute());
                }
#endif
            });

            #endregion
        }
    }
}
