using BusinessApp.Kernel;
using BusinessApp.Infrastructure;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using MSLogging = Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using System.Threading;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// The main registration class that glues the application together
    /// </summary>
    public class MainRegister : IBootstrapRegister
    {
        private readonly RegistrationOptions options;
        private readonly MSLogging.ILoggerFactory loggerFactory;

        public MainRegister(RegistrationOptions options, MSLogging.ILoggerFactory loggerFactory)
        {
            this.options = options;
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

            context.Container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();

            RegisterSpecifications(context.Container);
            RegisterBatchSupport(context.Container);
            RegisterLocalization(context.Container);
            RegisterLogging(context.Container);
#if DEBUG
            RegisterValidators(context.Container);
#else
#if validation
            RegisterValidators(context.Container);
#endif
#endif
            RegisterRequestDecoratePipeline(context);
            RegisterAppHandlers(context.Container);
        }

        private void RegisterSpecifications(Container container)
        {
            container.Register(typeof(ILinqSpecificationBuilder<,>),
                typeof(AndLinqSpecificationBuilder<,>));

            container.Collection.Register(typeof(ILinqSpecificationBuilder<,>),
                options.RegistrationAssemblies);

            container.Collection.Append(typeof(ILinqSpecificationBuilder<,>),
                typeof(QueryOperatorSpecificationBuilder<,>));
        }

        private static void RegisterLocalization(Container container)
        {
            container.RegisterConditional(typeof(IStringLocalizer),
                c => typeof(StringLocalizer<>).MakeGenericType(c.Consumer!.ImplementationType),
                Lifestyle.Singleton,
                c => c.HasConsumer);
            container.RegisterConditional(typeof(IStringLocalizer),
                c => typeof(StringLocalizer<>).MakeGenericType(typeof(Unit)),
                Lifestyle.Singleton,
                c => !c.HasConsumer);
        }

        private static void RegisterRequestDecoratePipeline(RegistrationContext context)
        {
            var serviceType = typeof(IRequestHandler<,>);
            var container = context.Container;

            // Apply the auth decorator as an actual service to run first in the pipeline
            // because we want it to only run once as early as possible once
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

            #region Query Pipeline

            context.Container.RegisterDecorator(
                serviceType,
                typeof(EntityNotFoundQueryDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(InstanceCacheQueryDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));

            #endregion

            // Apply the group decorator as an actual service second in the pipeline
            // for macro requests
#if DEBUG
            context.Container.RegisterConditional(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(MacroBatchRequestAdapter<,,>)));
#else
#if macro
            context.Container.RegisterConditional(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(MacroBatchRequestAdapter<,,>)));
#endif
#endif

            // Need to mark where the group handler is injected so the scope decorator
            // can deocorate this handler and run requests generated by the grouper in parallel.
            // We cannot conditionally register the scope decorator because it has a Func<>
            // that creates new contexts only when registered as a decorator
            context.Container.RegisterConditional(
                serviceType,
                typeof(DummyScopedBatchRequestDelegator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(GroupedBatchRequestDecorator<,>)));

#if DEBUG
            context.Container.RegisterConditional(
                serviceType,
                typeof(DeadlockRetryRequestDecorator<,>),
                c => CanHandle(c)
                    &&
                    (
                        c.Consumer.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                        || c.Consumer.ImplementationType.IsTypeDefinition(typeof(DummyScopedBatchRequestDelegator<,>))
                    )
                    && !c.ServiceType.GetGenericArguments()[0].IsMacro());
#else
#if macro
            // run this only once not within a macro request, only the requests
            // that are generated from the macro
            context.Container.RegisterConditional(
                serviceType,
                typeof(DeadlockRetryRequestDecorator<,>),
                c => CanHandle(c)
                    &&
                    (
                        c.Consumer.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                        || c.Consumer.ImplementationType.IsTypeDefinition(typeof(DummyScopedBatchRequestDelegator<,>))
                    )
                    && !c.ServiceType.GetGenericArguments()[0].IsMacro());
#else
            context.Container.RegisterConditional(
                serviceType,
                typeof(DeadlockRetryRequestDecorator<,>),
                c => CanHandle(c)
                    &&
                    (
                        c.Consumer.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                        || c.Consumer.ImplementationType.IsTypeDefinition(typeof(DummyScopedBatchRequestDelegator<,>))
                    ));
#endif
#endif

            context.Container.RegisterConditional(
                serviceType,
                typeof(TransactionRequestDecorator<,>),
                c => CanHandle(c)
                    && !c.ServiceType.GetGenericArguments()[0].IsQueryType()
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>)));

            #region Event Pipeline Additions

            // decorate the inner most handler that is not a decorator running as a service
            // most of the time this would be the actual command handler
            context.Container.RegisterDecorator(
                serviceType,
                typeof(EventConsumingRequestDecorator<,>),
                c => !c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                    && !c.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>))
                    && !c.ImplementationType.IsTypeDefinition(typeof(TransactionRequestDecorator<,>))
#if DEBUG
                    && !c.ImplementationType.IsTypeDefinition(typeof(ValidationRequestDecorator<,>))
#else
#if validation
                    && !c.ImplementationType.IsTypeDefinition(typeof(ValidationRequestDecorator<,>))
#endif
#endif
            );


            #endregion

            #region Batch Additios

#if DEBUG
            context.Container.RegisterDecorator(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>)));
#else
#if validation
            context.Container.RegisterDecorator(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>)));
#endif
#endif

            context.Container.RegisterDecorator(
                serviceType,
                typeof(SimpleInjectorScopedBatchRequestProxy<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                    || c.ImplementationType.IsTypeDefinition(typeof(DummyScopedBatchRequestDelegator<,>)));

            // Apply this as a decorator for batch requests before the ScopeBatchRequestProxy
            // Need the conditional registration for macro requests
            context.Container.RegisterDecorator(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));

#if DEBUG
            context.Container.RegisterConditional(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => CanHandle(c)
                    && (
                        c.Consumer.ImplementationType.IsTypeDefinition(typeof(BatchRequestAdapter<,>))
                        || c.Consumer.ImplementationType.IsTypeDefinition(typeof(MacroBatchRequestAdapter<,,>))
                        || c.ServiceType.GetGenericArguments()[0].IsMacro()
                    ));
#else
#if validation
#if macro
            // register this conditionally too so we can inject it at certain points
            // that need new validation not handled by the decoration registration
            context.Container.RegisterConditional(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => CanHandle(c)
                    && (
                        c.Consumer.ImplementationType.IsTypeDefinition(typeof(BatchRequestAdapter<,>))
                        || c.Consumer.ImplementationType.IsTypeDefinition(typeof(MacroBatchRequestAdapter<,,>))
                        || c.ServiceType.GetGenericArguments()[0].IsMacro()
                    ));
#else
            // register this conditionally too so we can inject it at certain points
            // that need new validation not handled by the decoration registration
            context.Container.RegisterConditional(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(BatchRequestAdapter<,>)));
#endif
#endif
#endif
            #endregion
        }

        private static bool CanHandle(PredicateContext context)
            => !context.Handled
                && context.HasConsumer
                && context.Consumer.ImplementationType != context.ImplementationType;


#if DEBUG
        private void RegisterValidators(Container container)
        {
            var validatorTypes = container.GetTypesToRegister(
                typeof(IValidator<>),
                options.RegistrationAssemblies
            );

            if (!validatorTypes.Any())
            {
                container.Collection.Append(typeof(IValidator<>), typeof(NullValidator<>));
            }
            else
            {
                foreach (var type in validatorTypes)
                {
                    container.Collection.Append(typeof(IValidator<>), type);
                }
            }

            container.Register(typeof(IValidator<>),
                typeof(CompositeValidator<>),
                Lifestyle.Singleton);
        }
#else
#if validation
        private void RegisterValidators(Container container)
        {
            var validatorTypes = container.GetTypesToRegister(
                typeof(IValidator<>),
                options.RegistrationAssemblies
            );

            if (!validatorTypes.Any())
            {
                container.Collection.Append(typeof(IValidator<>), typeof(NullValidator<>));
            }
            else
            {
                foreach (var type in validatorTypes)
                {
                    container.Collection.Append(typeof(IValidator<>), type);
                }
            }

            container.Register(typeof(IValidator<>),
                typeof(CompositeValidator<>),
                Lifestyle.Singleton);
        }
#endif
#endif

        private void RegisterBatchSupport(Container container)
        {
            container.Register(typeof(IBatchGrouper<>), options.RegistrationAssemblies);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);
        }

        private void RegisterLogging(Container container)
        {
            if (options.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                container.RegisterSingleton<ILogEntryFormatter, StringLogEntryFormatter>();
            }
            else
            {
                container.RegisterSingleton<ILogEntryFormatter, SerializedLogEntryFormatter>();
            }

            container.RegisterSingleton<ILogger, MicrosoftLoggerAdapter>();
            container.RegisterInstance(
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

            bool HasConcereteType(Type type)
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

#if DEBUG
            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchRequestAdapter<,,>),
                ctx => !ctx.Handled);
#else
#if macro
            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchRequestAdapter<,,>),
                ctx => !ctx.Handled);
#endif
#endif

            container.RegisterConditional(typeof(IRequestHandler<,>),
                c => CreateRequestHandler(c)!,
                Lifestyle.Scoped,
                c => !c.Handled && HasConcereteType(c.ServiceType));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(SingleQueryRequestAdapter<,>),
                c => CanHandle(c)
                    && c.ServiceType.GetGenericArguments()[0].IsQueryType()
                    && !c.ServiceType.GetGenericArguments()[1].IsTypeDefinition(typeof(EnvelopeContract<>))
                    && !c.ServiceType.GetGenericArguments()[1].IsGenericIEnumerable());

            container.RegisterConditional(typeof(IRequestHandler<,>),
                typeof(NoBusinessLogicRequestHandler<>),
                Lifestyle.Scoped,
                c => CanHandle(c) && !c.ServiceType.GetGenericArguments()[0].IsQueryType());
        }

#if DEBUG
        private sealed class NullValidator<T> : IValidator<T>
            where T : notnull
        {
            public Task<Result<Unit, Exception>> ValidateAsync(T instance, CancellationToken cancelToken)
                => Task.FromResult(Result.Ok());
        }
#else
#if validation
        private sealed class NullValidator<T> : IValidator<T>
            where T : notnull
        {
            public Task<Result<Unit, Exception>> ValidateAsync(T instance, CancellationToken cancelToken)
                => Task.FromResult(Result.Ok());
        }
#endif
#endif

        // Marker handler so the ScopedBatchRequestProxy can be registered
        private sealed class DummyScopedBatchRequestDelegator<T, R> : IRequestHandler<IEnumerable<T>, R>
            where T : notnull
        {
            private readonly IRequestHandler<IEnumerable<T>, R> inner;

            public DummyScopedBatchRequestDelegator(IRequestHandler<IEnumerable<T>, R> inner)
                => this.inner = inner.NotNull().Expect(nameof(inner));

            public Task<Result<R, Exception>> HandleAsync(IEnumerable<T> request,
                CancellationToken cancelToken) => inner.HandleAsync(request, cancelToken);
        }
    }
}
