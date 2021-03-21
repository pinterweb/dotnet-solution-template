namespace BusinessApp.WebApi
{
    using System;
    using System.Linq;
    using BusinessApp.App;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using SimpleInjector;

    public static partial class Bootstrapper
    {
        public static void RegisterServices(Container container,
            BootstrapOptions options,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory)
        {
            var bootstrapContainer = SetupBootstrapContainer(options, env, loggerFactory);

            RegisterBootstrapDecorators(bootstrapContainer);

            bootstrapContainer.Verify();

            var bootstrap = bootstrapContainer.GetInstance<IBootstrapRegister>();
            var regContext = new RegistrationContext { Container = container };

            bootstrap.Register(regContext);

            var serviceType = typeof(IRequestHandler<,>);

            var pipeline = regContext.GetPipelineBuilder(serviceType);

            foreach (var d in pipeline.Build().Reverse())
            {
                Predicate<DecoratorPredicateContext> filter = d.Item2.ScopeBehavior switch
                {
                    ScopeBehavior.Inner => (ctx) => IsInnerScope(ctx),
                    ScopeBehavior.Outer => (ctx) => IsOuterScope(ctx),
                    _ => (ctx) => true
                };

                var filter2 = d.Item2.RequestType switch
                {
                    RequestType.Command => (ctx) => filter(ctx) &&
                        !ctx.ServiceType.GetGenericArguments()[0].IsQueryType(),
                    RequestType.Query => (ctx) => filter(ctx) &&
                        ctx.ServiceType.GetGenericArguments()[0].IsQueryType(),
                    _ => filter,
                };

                var filter3 = d.Item2.ServiceFilter switch
                {
                    null => filter2,
                    _ => (ctx) => filter2(ctx) && d.Item2.ServiceFilter(ctx.ServiceType)
                };

                container.RegisterDecorator(
                    serviceType,
                    d.Item1,
                    d.Item2.Lifetime.MapLifestyle(),
                    filter3);
            }

            // XXX make sure this is absolultey last
            container.RegisterDecorator(typeof(IHttpRequestHandler<,>),
                typeof(HttpResponseDecorator<,>));

        }

        private static Container SetupBootstrapContainer(BootstrapOptions options,
            IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var bootstrapContainer = new Container();

            bootstrapContainer.Register<IBootstrapRegister, MainRegister>();
            bootstrapContainer.RegisterInstance(options);
            bootstrapContainer.RegisterInstance(env);
            bootstrapContainer.RegisterInstance(loggerFactory);

            return bootstrapContainer;
        }

        private static void RegisterBootstrapDecorators(Container container)
        {
            var bootstrapDecorators = container
                .GetTypesToRegister(typeof(IBootstrapRegister),
                    new[] { typeof(Startup).Assembly },
                    new TypesToRegisterOptions
                    {
                        IncludeDecorators = true
                    })
                .Where(type => type != typeof(MainRegister));

            foreach (var type in bootstrapDecorators)
            {
                container.RegisterDecorator(typeof(IBootstrapRegister),type);
            }
        }

        private static bool IsInnerScope(DecoratorPredicateContext ctx)
        {
            var implType = ctx.ImplementationType;

            return !implType.IsConstructedGenericType ||
            (
                implType.GetGenericTypeDefinition() == typeof(BatchRequestAdapter<,>)
                || implType.GetGenericTypeDefinition() == typeof(MacroBatchProxyRequestHandler<,>)
                || implType.GetGenericTypeDefinition() == typeof(SingleQueryRequestAdapter<,,>)
                || implType.GetGenericTypeDefinition() == typeof(NoBusinessLogicRequestHandler<>)
            );
        }

        private static bool IsOuterScope(DecoratorPredicateContext ctx)
        {
                var implType = ctx.ImplementationType;

                return !implType.IsConstructedGenericType ||
                (
                    implType.GetGenericTypeDefinition() != typeof(BatchProxyRequestHandler<,,>)
                    && implType.GetGenericTypeDefinition() != typeof(MacroBatchProxyRequestHandler<,>)
                );
        }
    }
 }
