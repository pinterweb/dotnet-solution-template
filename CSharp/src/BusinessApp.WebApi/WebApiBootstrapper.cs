namespace BusinessApp.WebApi
{
    using System.Reflection;
    using System.Security.Principal;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using SimpleInjector;

    /// <summary>
    /// Allows registering all types that are defined in the web api layer
    /// </summary>
    public static class WebApiBootstrapper
    {
        public static readonly Assembly Assembly = typeof(Startup).Assembly;

        public static Container Bootstrap(IApplicationBuilder app,
            IWebHostEnvironment env,
            Container container,
            BootstrapOptions options)
        {
            DomainLayerBoostrapper.Bootstrap(container);
            AppLayerBootstrapper.Bootstrap(container, env, options);
            DataLayerBootstrapper.Bootstrap(container, options);

            container.RegisterDecorator(typeof(IResourceHandler<,>), typeof(ResourceNotFoundRequestDecorator<,>));

#if json
            Json.Bootstrapper.Bootstrap(container);
#endif

            container.RegisterSingleton<IPrincipal, HttpUserContext>();
            container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();

            container.Register(typeof(IResourceHandler<,>), Assembly);
            container.RegisterConditional(
                typeof(IResourceHandler<,>),
                typeof(EnvelopeQueryResourceHandler<,>),
                ctx => !ctx.Handled);
            container.RegisterConditional(
                typeof(IResourceHandler<,>),
                typeof(QueryResourceHandler<,>),
                ctx => !ctx.Handled
            );
            container.RegisterConditional(
                typeof(IResourceHandler<,>),
                typeof(CommandResourceHandler<>),
                ctx => !ctx.Handled
            );

            return container;
        }
    }
}
