namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using SimpleInjector;

    /// <summary>
    /// Creates your routes
    /// </summary>
    public static class RoutingBootstrapper
    {
        public static void Bootstrap(Container container, IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);

            #region TODO APIS HERE

            //routeBuilder.MapGet("/api/<aggregate>", async ctx =>
            //    await container
            //        .GetInstance<IResourceHandler<SomeQuery, IEnumerable<BusinessContract>>>()
            //        .HandleAsync(ctx, default)
            //);
            //routeBuilder.MapGet("/api/<aggregate>/{id:long}", async ctx =>
            //    await container
            //        .GetInstance<IResourceHandler<SomeQuery, BusinessContract>>()
            //        .HandleAsync(ctx, default)
            //);
            //routeBuilder.MapPost("/api/<aggregate>", async ctx =>
            //    await container
            //        .GetInstance<IResourceHandler<BusinessCommand, BusinessCommand>>()
            //        .HandleAsync(ctx, default)
            //);
            //routeBuilder.MapPut("/api/<aggregate>/{id:long}", async ctx =>
            //    await container
            //        .GetInstance<IResourceHandler<BusinessCommand, BusinessCommand>>()
            //        .HandleAsync(ctx, default)
            //);


            #endregion

            app.UseRouter(routeBuilder.Build());
        }
    }
}
