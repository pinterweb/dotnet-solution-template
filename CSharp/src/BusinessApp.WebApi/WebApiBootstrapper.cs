namespace BusinessApp.WebApi
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Principal;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Builder;
    using SimpleInjector;

    /// <summary>
    /// Allows registering all types that are defined in the web api layer
    /// </summary>
    public static class WebApiBootstrapper
    {
        public static readonly Assembly Assembly = typeof(Startup).Assembly;

        public static Container Bootstrap(IApplicationBuilder app, Container container)
        {
            DomainLayerBoostrapper.Bootstrap(container);
            AppLayerBootstrapper.Bootstrap(container);
            DataLayerBootstrapper.Bootstrap(container);

#if json
            Json.Bootstrapper.Bootstrap(container);
#endif

            container.RegisterSingleton<IPrincipal, HttpUserContext>();
            container.RegisterInstance<ILogger>(new TraceLogger());
            container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();

            container.Register(typeof(IResourceHandler<,>), Assembly);
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
            container.RegisterDecorator(typeof(IResourceHandler<,>), typeof(ResourceNotFoundRequestDecorator<,>));

            RoutingBootstrapper.Bootstrap(container, app);

            return container;
        }

        private sealed class TraceLogger : ILogger
        {
            public TraceLogger()
            {
                Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
            }

            public void Log(LogEntry entry) => Trace.WriteLine(entry.Exception);
        }
    }
}
