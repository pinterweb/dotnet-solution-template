namespace BusinessApp.WebApi
{
    using System.Reflection;
    using System.Security.Principal;
    using BusinessApp.Domain;
    using BusinessApp.App;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using SimpleInjector;
    using BusinessApp.WebApi.ProblemDetails;
#if !json
    using System.IO;
#endif

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

#if json
            Json.Bootstrapper.Bootstrap(container);
#else
            container.RegisterSingleton<ISerializer, NullSerializer>();
#endif

            container.RegisterSingleton<IPrincipal, HttpUserContext>();
            container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();
            container.RegisterSingleton<IAppScope, SimpleInjectorWebApiAppScope>();
            container.RegisterSingleton<IResponseWriter, HttpResponseWriter>();
            container.RegisterSingleton<IProblemDetailFactory, ProblemDetailFactory>();
            container.RegisterDecorator<IProblemDetailFactory, ProblemDetailFactoryHttpDecorator>(
                Lifestyle.Singleton);

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
#if !json
        private sealed class NullSerializer : ISerializer
        {
            public T Deserialize<T>(Stream serializationStream)
            {
                throw new System.NotImplementedException();
            }

            public void Serialize(Stream serializationStream, object graph)
            {
                throw new System.NotImplementedException();
            }
        }
#endif
    }
}
