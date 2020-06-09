namespace BusinessApp.WebApi
{
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

            #region TODO APIS HERE

            app.UseEndpoints(endpoint =>
            {
                var endpoints = new IEndpointConventionBuilder[]
                {
                    //endpoint.MapGet("/api/<aggregate>", async ctx =>
                    //    await container
                    //        .GetInstance<IResourceHandler<SomeQuery, IEnumerable<BusinessContract>>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapGet("/api/<aggregate>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IResourceHandler<SomeQuery, BusinessContract>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapPost("/api/<aggregate>", async ctx =>
                    //    await container
                    //        .GetInstance<IResourceHandler<BusinessCommand, BusinessCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapPut("/api/<aggregate>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IResourceHandler<BusinessCommand, BusinessCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                    //endpoint.MapDelete("/api/<aggregate>/{id:long}", async ctx =>
                    //    await container
                    //        .GetInstance<IResourceHandler<DeleteCommand, DeleteCommand>>()
                    //        .HandleAsync(ctx, default)
                    //),
                };
            });

            #endregion
        }
    }
}
