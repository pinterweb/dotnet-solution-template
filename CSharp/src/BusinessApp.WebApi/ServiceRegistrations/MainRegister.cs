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
    using Microsoft.Extensions.Localization;

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
            RegisterRequestDecoratePipeline(context);
            RegisterAppHandlers(context.Container);
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

            context.Container
                .RegisterSingleton<IHttpRequestHandler, SimpleInjectorHttpRequestHandler>();

            context.Container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestHandler<,>),
                ctx => !ctx.Handled);

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler),
                typeof(HttpRequestBodyAnalyzer),
                Lifestyle.Singleton
            );

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler),
                typeof(HttpRequestLoggingDecorator),
                Lifestyle.Singleton
            );

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestLoggingDecorator<,>));

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpResponseDecorator<,>));
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
            container.RegisterDecorator<IProblemDetailFactory, LocalizedProblemDetailFactory>();
            container.RegisterConditional(typeof(IStringLocalizer),
                c => typeof(StringLocalizer<>).MakeGenericType(c.Consumer!.ImplementationType),
                Lifestyle.Singleton,
                c => c.HasConsumer);
            container.RegisterConditional(typeof(IStringLocalizer),
                c => typeof(StringLocalizer<>).MakeGenericType(typeof(Unit)),
                Lifestyle.Singleton,
                c => !c.HasConsumer);
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

            bool HasConcereteType(Type type )
            {
                return container.GetRootRegistrations()
                    .Any(reg => reg.ServiceType.GetInterfaces().Any(i => i == type));
            }

            Type? CreateRequestHandler(TypeFactoryContext c)
            {
                var requestType = c.ServiceType.GetGenericArguments()[0];
                var responseType = c.ServiceType.GetGenericArguments()[1];
                var concreteType = container.GetRootRegistrations()
                    .FirstOrDefault(reg =>
                        reg.ServiceType.GetInterfaces().Any(i => i == c.ServiceType))?
                    .ServiceType;

                return concreteType;
            }

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(BatchRequestAdapter<,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchRequestAdapter<,,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(typeof(IRequestHandler<,>),
                c => CreateRequestHandler(c)!,
                Lifestyle.Scoped,
                c => !c.Handled && HasConcereteType(c.ServiceType));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(SingleQueryRequestAdapter<,>),
                c => CanHandle(c)
                    && c.ServiceType.GetGenericArguments()[0].IsQueryType()
                    && !c.ServiceType.GetGenericArguments()[1].IsGenericIEnumerable());

            container.RegisterConditional(typeof(IRequestHandler<,>),
                typeof(NoBusinessLogicRequestHandler<>),
                Lifestyle.Scoped,
                c => CanHandle(c) && !c.ServiceType.GetGenericArguments()[0].IsQueryType());
        }

        private void RegisterRequestDecoratePipeline(RegistrationContext context)
        {
            var serviceType = typeof(IRequestHandler<,>);
            var container = context.Container;

            static bool IsTypeDefinition(Type actual, Type test)
            {
                return actual.IsGenericType
                    && actual.GetGenericTypeDefinition() == test;
            };

            #region Query Pipeline

            container.RegisterConditional(
                serviceType,
                typeof(AuthorizationRequestDecorator<,>),
                c => (!c.Handled && !c.HasConsumer)
                    || (
                        !c.Handled
                        && c.Consumer
                            .ImplementationType
                            .GetInterfaces()
                            .All(i => i.IsGenericType
                                && i.GetGenericTypeDefinition() != typeof(IRequestHandler<,>))
                        ));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(EntityNotFoundQueryDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(InstanceCacheQueryDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(EFTrackingQueryDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(RequestExceptionDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));

            context.Container.RegisterConditional(
                serviceType,
                typeof(DeadlockRetryRequestDecorator<,>),
                c => CanHandle(c)
                    && IsTypeDefinition(c.Consumer.ImplementationType, typeof(AuthorizationRequestDecorator<,>))
                    && !c.ServiceType.GetGenericArguments()[0].IsMacro()
                    ||
                    (
                        CanHandle(c)
                        && IsTypeDefinition(c.Consumer.ImplementationType, typeof(MacroBatchRequestAdapter<,,>))
                    ));


            #endregion

            #region Single Request Pipeline Additions

            context.Container.RegisterConditional(
                serviceType,
                typeof(TransactionRequestDecorator<,>),
                c => CanHandle(c)
                    && !c.ServiceType.GetGenericArguments()[0].IsQueryType()
                    && IsTypeDefinition(c.Consumer.ImplementationType, typeof(DeadlockRetryRequestDecorator<,>)));

            #endregion

            #region Event Stream Pipeline Additions

            // decorate the inner most handler
            context.Container.RegisterDecorator(
                serviceType,
                typeof(EventConsumingRequestDecorator<,>),
                c => !IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>))
                    && !IsTypeDefinition(c.ImplementationType, typeof(DeadlockRetryRequestDecorator<,>))
                    && !IsTypeDefinition(c.ImplementationType, typeof(TransactionRequestDecorator<,>))
                    && !IsTypeDefinition(c.ImplementationType, typeof(ValidationRequestDecorator<,>))
            );

            #endregion

            #region Batch & Macro Additions

            context.Container.RegisterDecorator(
                serviceType,
                typeof(ScopedBatchRequestProxy<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(DeadlockRetryRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(DeadlockRetryRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(DeadlockRetryRequestDecorator<,>)));

            context.Container.RegisterConditional(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => CanHandle(c)
                    && (
                        IsTypeDefinition(c.Consumer.ImplementationType, typeof(BatchRequestAdapter<,>))
                        || IsTypeDefinition(c.Consumer.ImplementationType, typeof(MacroBatchRequestAdapter<,,>))
                        || c.ServiceType.GetGenericArguments()[0].IsMacro()
                    ));

            #endregion
        }

        private static bool CanHandle(PredicateContext context)
        {
            return !context.Handled
                && context.HasConsumer
                && context.Consumer.ImplementationType != context.ImplementationType;
        }

        /// <summary>
        /// Null object pattern. When no authorization is used on a request
        /// </summary>
        private sealed class NullAuthorizer<T> : IAuthorizer<T>
            where T : notnull
        {
            public void AuthorizeObject(T instance)
            {}
        }
    }
}
