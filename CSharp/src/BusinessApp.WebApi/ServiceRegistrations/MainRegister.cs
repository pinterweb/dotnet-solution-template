namespace BusinessApp.WebApi
{
    using BusinessApp.Domain;
    using BusinessApp.Data;
    using BusinessApp.App;
    using SimpleInjector;
    using BusinessApp.WebApi.ProblemDetails;
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.Security.Principal;
    using System.Collections.Generic;
    using System.Linq;

    public class MainRegister : IBootstrapRegister
    {
        private readonly BootstrapOptions options;
        private readonly IWebHostEnvironment env;

        public MainRegister(BootstrapOptions options, IWebHostEnvironment env)
        {
            this.options = options;
            this.env = env;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.Register<IEventRepository, EventRepository>();
            container.Collection.Register(typeof(IEventHandler<>), options.RegistrationAssemblies);
            container.Collection.Append(typeof(IEventHandler<>), typeof(DomainEventHandler<>));

            container.Register<IUnitOfWork, EventUnitOfWork>();

            container.Register<PostCommitRegister>();
            container.Register<IPostCommitRegister>(container.GetInstance<PostCommitRegister>);

            RegisterAuthorization(context.Container);
            RegisterBatchSupport(context.Container);
            RegisterWebApiErrorResponse(context.Container);
            RegisterLogging(context.Container);
            RegisterQueryHandling(context.Container);
            RegisterValidators(context.Container);
            RegisterWebApiServices(context.Container);
            RegisterAppHandlers(context.Container);
            RegisterDecoratePipeline(context);
        }

        private void RegisterWebApiServices(Container container)
        {
            container.RegisterSingleton<IPrincipal, HttpUserContext>();
            container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();
            container.RegisterSingleton<IAppScope, SimpleInjectorWebApiAppScope>();

            container.Register(typeof(IHttpRequestHandler<,>), options.RegistrationAssemblies);

            container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(EnvelopeQueryResourceHandler<,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestHandler<,>),
                ctx => !ctx.Handled);

            container.RegisterDecorator(typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestLoggingDecorator<,>));
        }

        private void RegisterValidators(Container container)
        {
            var validatorTypes = container.GetTypesToRegister(
                typeof(IValidator<>),
                options.RegistrationAssemblies
            );

            foreach (var type in validatorTypes)
            {
                container.Collection.Append(typeof(IValidator<>), type);
            }

            container.Register(typeof(IValidator<>),
                typeof(CompositeValidator<>),
                Lifestyle.Singleton);
        }

        private void RegisterQueryHandling(Container container)
        {
            container.Register(typeof(IQueryVisitorFactory<,>),
                typeof(CompositeQueryVisitorBuilder<,>));

            container.Register(typeof(ILinqSpecificationBuilder<,>),
                typeof(AndSpecificationBuilder<,>));

            container.Collection.Register(typeof(ILinqSpecificationBuilder<,>),
                options.RegistrationAssemblies);

            container.Collection.Register(typeof(IQueryVisitor<>), options.RegistrationAssemblies);

            container.RegisterConditional(
                typeof(IQueryVisitor<>),
                typeof(NullQueryVisitor<>), ctx => !ctx.Handled);

            container.Collection.Append(typeof(ILinqSpecificationBuilder<,>), typeof(QueryOperatorSpecificationBuilder<,>));

            container.Collection.Register(typeof(IQueryVisitorFactory<,>), new[]
            {
                typeof(AndSpecificationBuilder<,>),
                typeof(ConstructedQueryVisitorFactory<,>),
            });
        }

        private void RegisterAuthorization(Container container)
        {
            container.RegisterConditional(
                typeof(IAuthorizer<>),
                typeof(NullAuthorizer<>),
                c => !c.Handled);
        }

        private void RegisterBatchSupport(Container container)
        {
            container.Register(typeof(IBatchMacro<,>), options.RegistrationAssemblies);
            container.Register(typeof(IBatchGrouper<>), options.RegistrationAssemblies);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);
        }

        private void RegisterWebApiErrorResponse(Container container)
        {
            container.RegisterInstance(ProblemDetailOptionBootstrap.KnownProblems);

            container.RegisterSingleton<IProblemDetailFactory, ProblemDetailFactory>();
        }

        private void RegisterLogging(Container container)
        {
            container.Register(typeof(ILogger), typeof(CompositeLogger), Lifestyle.Singleton);

            if (env.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                container.Collection.Append<ILogger, TraceLogger>();
            }

            container.RegisterSingleton<ILogEntryFormatter, SerializedLogEntryFormatter>();
            container.RegisterInstance<IFileProperties>(new RollingFileProperties
            {
                Name = options.LogFilePath ?? $"{env.ContentRootPath}/app.log"
            });

            container.RegisterSingleton<ConsoleLogger>();

            container.RegisterSingleton<FileLogger>();

            container.Collection.Append<ILogger>(() =>
                new BackgroundLogDecorator(
                    container.GetInstance<FileLogger>(),
                    container.GetInstance<ConsoleLogger>()),
                Lifestyle.Singleton);

        }

        private void RegisterAppHandlers(Container container)
        {
            IEnumerable<Type> RegisterConcreteHandlers()
            {
                var handlers = container.GetTypesToRegister(typeof(IRequestHandler<,>),
                    options.RegistrationAssemblies,
                    new TypesToRegisterOptions
                    {
                        IncludeGenericTypeDefinitions = true,
                        IncludeComposites = false,
                    });

                foreach (var handlerType in handlers)
                {
                    container.Register(handlerType);
                }

                return handlers;
            }

            var handlers = RegisterConcreteHandlers();

            Type CreateQueryType(TypeFactoryContext c)
            {
                var requestType = c.ServiceType.GetGenericArguments()[0];
                var responseType = c.ServiceType.GetGenericArguments()[1];
                var concreteType = container.GetRootRegistrations()
                                .FirstOrDefault(reg => reg.ServiceType.GetInterfaces().Any(i => i == c.ServiceType))?
                                .ServiceType;

                if (concreteType != null) return concreteType;

                var fallbackType = typeof(IRequestHandler<,>)
                    .MakeGenericType(requestType,
                        typeof(IEnumerable<>).MakeGenericType(responseType));

                var fallbackRegistration = container.GetRegistration(fallbackType);

                if (fallbackRegistration == null)
                {
                    throw new BusinessAppWebApiException(
                        $"No query handler is setup for '{requestType.Name}'. Please set one up.");
                }

                return typeof(SingleQueryRequestAdapter<,,>).MakeGenericType(
                    fallbackRegistration.Registration.ImplementationType,
                    requestType,
                    responseType);
            }

            bool HasConcereteType(Type type )
            {
                return container.GetRootRegistrations()
                    .Any(reg => reg.ServiceType.GetInterfaces().Any(i => i == type));
            }

            Type CreateRequestHandler(TypeFactoryContext c)
            {
                var requestType = c.ServiceType.GetGenericArguments()[0];
                var responseType = c.ServiceType.GetGenericArguments()[1];
                var concreteType = container.GetRootRegistrations()
                    .FirstOrDefault(reg =>
                        reg.ServiceType.GetInterfaces().Any(i => i == c.ServiceType))?
                    .ServiceType;

                return c.Consumer?.ImplementationType.GetGenericTypeDefinition() == typeof(BatchRequestAdapter<,>)
                    ? typeof(BatchProxyRequestHandler<,,>).MakeGenericType(concreteType, requestType, responseType)
                    : concreteType;
            }

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchProxyRequestHandler<,>),
                ctx => ctx.Consumer?.ImplementationType.GetGenericTypeDefinition() == typeof(MacroBatchRequestAdapter<,,>));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(BatchRequestAdapter<,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchRequestAdapter<,,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(typeof(IRequestHandler<,>),
                c => CreateRequestHandler(c),
                Lifestyle.Scoped,
                c => !c.Handled && HasConcereteType(c.ServiceType));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                c => CreateQueryType(c),
                Lifestyle.Singleton,
                c => !c.Handled
                    && typeof(IQuery).IsAssignableFrom(c.ServiceType.GetGenericArguments()[0])
                    // to prevent stackoverflow, this is only looking for queries
                    // that return a signal value
                    &&
                    (
                        !c.ServiceType.GetGenericArguments()[1].IsGenericType
                        || c.ServiceType.GetGenericArguments()[1].GetGenericTypeDefinition() != typeof(IEnumerable<>)
                    ));
        }

        private void RegisterDecoratePipeline(RegistrationContext context)
        {
            static bool IsOuterScope(DecoratorPredicateContext ctx)
            {
                var implType = ctx.ImplementationType;

                return !implType.IsConstructedGenericType ||
                (
                    implType.GetGenericTypeDefinition() != typeof(BatchProxyRequestHandler<,,>)
                    && implType.GetGenericTypeDefinition() != typeof(MacroBatchProxyRequestHandler<,>)
                );
            }

            static bool IsInnerScope(DecoratorPredicateContext ctx)
            {
                var implType = ctx.ImplementationType;

                return !implType.IsConstructedGenericType ||
                (
                    implType.GetGenericTypeDefinition() == typeof(BatchRequestAdapter<,>)
                    || implType.GetGenericTypeDefinition() == typeof(MacroBatchProxyRequestHandler<,>)
                    || implType.GetGenericTypeDefinition() == typeof(SingleQueryRequestAdapter<,,>)
                );
            }

            var serviceType = typeof(IRequestHandler<,>);

            var pipeline = context.GetPipelineBuilder(serviceType);

            // Request / Command Pipeline
            pipeline.RunOnce(typeof(RequestExceptionDecorator<,>))
                .RunOnce(typeof(AuthorizationRequestDecorator<,>))
                .RunOnce(typeof(InstanceCacheQueryDecorator<,>))
                .Run(typeof(ValidationRequestDecorator<,>))
                .RunOnce(typeof(EntityNotFoundQueryDecorator<,>))
                .Run(typeof(GroupedBatchRequestDecorator<,>))
                .Run(typeof(ScopedBatchRequestProxy<,>))
                .RunOnce(typeof(DeadlockRetryRequestDecorator<,>))
                .RunOnce(typeof(TransactionRequestDecorator<,>), RequestType.Command);

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

                context.Container.RegisterDecorator(
                    serviceType,
                    d.Item1,
                    d.Item2.Lifetime.MapLifestyle(),
                    filter2);
            }
        }

        /// <summary>
        /// Null object pattern. When no authorization is used on a request
        /// </summary>
        private sealed class NullAuthorizer<T> : IAuthorizer<T>
        {
            public void AuthorizeObject(T instance)
            {}
        }
    }
}
