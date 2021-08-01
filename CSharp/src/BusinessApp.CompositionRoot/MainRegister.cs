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
            RegisterLocalization(context.Container);
            RegisterLogging(context.Container);
#if DEBUG
            RegisterValidators(context.Container);
#elif validation
            RegisterValidators(context.Container);
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

            context.Container.RegisterDecorator(
                serviceType,
                typeof(EntityNotFoundQueryDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(InstanceCacheQueryDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));

            // Apply the group decorator as an actual service second in the pipeline
            // for macro requests
#if DEBUG
            context.Container.RegisterConditional(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(MacroBatchRequestAdapter<,,>)));
#else
#if hasbatch
#if macro
            context.Container.RegisterConditional(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(MacroBatchRequestAdapter<,,>)));
#else
            context.Container.RegisterConditional(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => CanHandle(c));
#endif
#endif
#endif

            // Need to mark where the group handler is injected so the scope decorator
            // can deocorate this handler and run requests generated by the grouper in parallel.
            // We cannot conditionally register the scope decorator because it has a Func<>
            // that creates new contexts only when registered as a decorator
#if DEBUG
            context.Container.RegisterConditional(
                serviceType,
                typeof(DummyScopedBatchRequestDelegator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(GroupedBatchRequestDecorator<,>)));
#elif hasbatch
            context.Container.RegisterConditional(
                serviceType,
                typeof(DummyScopedBatchRequestDelegator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(GroupedBatchRequestDecorator<,>)));
#endif

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
#elif macro
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
#elif hasbatch
            context.Container.RegisterConditional(
                serviceType,
                typeof(DeadlockRetryRequestDecorator<,>),
                c => CanHandle(c)
                    &&
                    (
                        c.Consumer.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                        || c.Consumer.ImplementationType.IsTypeDefinition(typeof(DummyScopedBatchRequestDelegator<,>))
                    ));
#else
            context.Container.RegisterConditional(
                serviceType,
                typeof(DeadlockRetryRequestDecorator<,>),
                c => CanHandle(c)
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));
#endif

            context.Container.RegisterConditional(
                serviceType,
                typeof(TransactionRequestDecorator<,>),
                c => CanHandle(c)
                    && !c.ServiceType.GetGenericArguments()[0].IsQueryType()
                    && c.Consumer.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>)));

#if DEBUG
            context.Container.RegisterDecorator(
                serviceType,
                typeof(EventConsumingRequestDecorator<,>),
                c => !c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                    && !c.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>))
                    && !c.ImplementationType.IsTypeDefinition(typeof(TransactionRequestDecorator<,>))
                    && !c.ImplementationType.IsTypeDefinition(typeof(ValidationRequestDecorator<,>)));
#elif validation
            // decorate the inner most handler that is not a decorator running as a service
            // most of the time this would be the actual command handler

            context.Container.RegisterDecorator(
                serviceType,
                typeof(EventConsumingRequestDecorator<,>),
                c => !c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                    && !c.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>))
                    && !c.ImplementationType.IsTypeDefinition(typeof(TransactionRequestDecorator<,>))
                    && !c.ImplementationType.IsTypeDefinition(typeof(ValidationRequestDecorator<,>))
            );
#endif


#if DEBUG
            context.Container.RegisterDecorator(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>)));
#elif validation
            context.Container.RegisterDecorator(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(DeadlockRetryRequestDecorator<,>)));
#endif

#if DEBUG
            context.Container.RegisterDecorator(
                serviceType,
                typeof(SimpleInjectorScopedBatchRequestProxy<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                    || c.ImplementationType.IsTypeDefinition(typeof(DummyScopedBatchRequestDelegator<,>)));
#elif hasbatch
            context.Container.RegisterDecorator(
                serviceType,
                typeof(SimpleInjectorScopedBatchRequestProxy<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>))
                    || c.ImplementationType.IsTypeDefinition(typeof(DummyScopedBatchRequestDelegator<,>)));
#endif

#if DEBUG
            context.Container.RegisterDecorator(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));
#elif hasbatch
            // Apply this as a decorator for batch requests before the ScopeBatchRequestProxy
            // Need the conditional registration for macro requests
            context.Container.RegisterDecorator(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));
#endif

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
#elif hasbatch
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
#elif validation
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

#if DEBUG
            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(BatchRequestAdapter<,>),
                ctx => !ctx.Handled);
#elif hasbatch
            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(BatchRequestAdapter<,>),
                ctx => !ctx.Handled);
#endif

#if DEBUG
            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchRequestAdapter<,,>),
                ctx => !ctx.Handled);
#elif macro
            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchRequestAdapter<,,>),
                ctx => !ctx.Handled);
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
#elif validation
        private sealed class NullValidator<T> : IValidator<T>
            where T : notnull
        {
            public Task<Result<Unit, Exception>> ValidateAsync(T instance, CancellationToken cancelToken)
                => Task.FromResult(Result.Ok());
        }
#endif

#if DEBUG
        private sealed class DummyScopedBatchRequestDelegator<T, R> : IRequestHandler<IEnumerable<T>, R>
            where T : notnull
        {
            private readonly IRequestHandler<IEnumerable<T>, R> inner;

            public DummyScopedBatchRequestDelegator(IRequestHandler<IEnumerable<T>, R> inner)
                => this.inner = inner.NotNull().Expect(nameof(inner));

            public Task<Result<R, Exception>> HandleAsync(IEnumerable<T> request,
                CancellationToken cancelToken) => inner.HandleAsync(request, cancelToken);
        }
#elif hasbatch
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
#endif
    }
}
