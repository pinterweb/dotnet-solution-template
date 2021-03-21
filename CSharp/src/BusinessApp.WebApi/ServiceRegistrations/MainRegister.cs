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
    using MSLogging = Microsoft.Extensions.Logging;

    public class MainRegister : IBootstrapRegister
    {
        private readonly BootstrapOptions options;
        private readonly IWebHostEnvironment env;
        private readonly MSLogging.ILoggerFactory loggerFactory;

        public MainRegister(BootstrapOptions options, IWebHostEnvironment env,
            MSLogging.ILoggerFactory loggerFactory)
        {
            this.options = options;
            this.env = env;
            this.loggerFactory = loggerFactory;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.RegisterSingleton(typeof(IEntityIdFactory<>), typeof(LongEntityIdFactory<>));
            container.Collection.Register(typeof(IEventHandler<>), options.RegistrationAssemblies);

            container.Register<IEventPublisherFactory, EventMetadataPublisherFactory>();
            container.Register<PostCommitRegister>();
            container.Register<IPostCommitRegister>(container.GetInstance<PostCommitRegister>);

            RegisterAuthorization(context.Container);
            RegisterBatchSupport(context.Container);
            RegisterWebApiErrorResponse(context.Container);
            RegisterLogging(context.Container);
            RegisterQueryHandling(context.Container);
            RegisterValidators(context.Container);
            RegisterWebApiServices(context);
            RegisterAppHandlers(context.Container);
            RegisterRequestDecoratePipeline(context);
        }

        private void RegisterWebApiServices(RegistrationContext context)
        {
            context.Container.RegisterSingleton<IPrincipal, HttpUserContext>();
            context.Container.RegisterDecorator<IPrincipal, AnonymousUser>();
            context.Container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();
            context.Container.RegisterSingleton<IAppScope, SimpleInjectorWebApiAppScope>();

            context.Container.Register(typeof(IHttpRequestHandler<,>), options.RegistrationAssemblies);

            context.Container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(EnvelopeQueryResourceHandler<,>),
                ctx => !ctx.Handled);

            context.Container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestHandler<,>),
                ctx => !ctx.Handled);

            var serviceType = typeof(IHttpRequestHandler<,>);
            var pipeline = context.GetPipelineBuilder(serviceType);

            pipeline.Run(typeof(HttpRequestLoggingDecorator<,>));
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
            if (env.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                container.RegisterSingleton<ILogEntryFormatter, StringLogEntryFormatter>();
            }
            else
            {
                container.RegisterSingleton<ILogEntryFormatter, SerializedLogEntryFormatter>();
            }

            container.RegisterSingleton<ILogger, MicrosoftLoggerAdapter>();
            container.RegisterInstance<MSLogging.ILogger>(
                loggerFactory.CreateLogger("BusinessApp"));
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
                ctx => ctx.HasConsumer
                    && ctx.Consumer.ImplementationType.GetGenericTypeDefinition() == typeof(MacroBatchRequestAdapter<,,>));

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

            container.RegisterConditional(typeof(IRequestHandler<,>),
                typeof(NoBusinessLogicRequestHandler<>),
                Lifestyle.Scoped,
                c => !c.Handled);
        }

        private void RegisterRequestDecoratePipeline(RegistrationContext context)
        {
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
                .RunOnce(typeof(TransactionRequestDecorator<,>), RequestType.Command)
                .Run(typeof(EventConsumingRequestDecorator<,>));

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
