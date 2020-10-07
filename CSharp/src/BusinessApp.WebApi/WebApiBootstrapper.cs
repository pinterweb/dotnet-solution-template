namespace BusinessApp.WebApi
{
    using System.Reflection;
    using System.Security.Principal;
    using BusinessApp.Domain;
    using BusinessApp.App;
    using BusinessApp.WebApi.ProblemDetails;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using SimpleInjector;
#if !json
    using System.IO;
    using System.Collections.Generic;
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
            // TODO need to register prior to Data layer. Probabl should put
            // all request handling in one bootstrap file instead of layers
            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(SingleQueryHandlerDelegator<,>),
                ctx => ctx.Handled);
            DomainLayerBoostrapper.Bootstrap(container);
            DataLayerBootstrapper.Bootstrap(container, options);
            AppLayerBootstrapper.Bootstrap(container, env, options);

#if json
            Json.Bootstrapper.Bootstrap(container);
#else
            container.RegisterSingleton<ISerializer, NullSerializer>();
            container.RegisterInstance(
                new HashSet<ProblemDetailOptions>(ProblemDetailOptionBootstrap.KnownProblems));
#endif

            container.RegisterSingleton<IPrincipal, HttpUserContext>();
            container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();
            container.RegisterSingleton<IAppScope, SimpleInjectorWebApiAppScope>();
            container.RegisterSingleton<IResponseWriter, HttpResponseWriter>();
            container.RegisterSingleton<IProblemDetailFactory, ProblemDetailFactory>();
            container.RegisterDecorator<IProblemDetailFactory, ProblemDetailFactoryHttpDecorator>(
                Lifestyle.Singleton);

            container.Register(typeof(IHttpRequestHandler<,>), Assembly);
            container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(EnvelopeQueryResourceHandler<,>),
                ctx => !ctx.Handled);
            container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestHandler<,>),
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
