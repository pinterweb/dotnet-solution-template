using BusinessApp.Domain;
using BusinessApp.Data;
using BusinessApp.App;
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
            container.Register(typeof(IRequestMapper<,>), options.RegistrationAssemblies);

            container.Register<IEventPublisherFactory, EventMetadataPublisherFactory>();
            container.Register<PostCommitRegister>();
            container.Register<IPostCommitRegister>(container.GetInstance<PostCommitRegister>);

            context.Container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();
            context.Container.Register<IProcessManager, SimpleInjectorProcessManager>();

            RegisterAuthorization(context.Container);
            RegisterBatchSupport(context.Container);
            RegisterLocalization(context.Container);
            RegisterLogging(context.Container);
            RegisterQueryHandling(context.Container);
            RegisterValidators(context.Container);
            RegisterRequestDecoratePipeline(context);
            RegisterAppHandlers(context.Container);
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

        private void RegisterLocalization(Container container)
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
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(InstanceCacheQueryDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(EFTrackingQueryDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));

            #endregion

            // Apply the group decorator as an actual service second in the pipeline
            // for macro requests
            context.Container.RegisterConditional(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => CanHandle(c)
                    && IsTypeDefinition(c.Consumer.ImplementationType, typeof(MacroBatchRequestAdapter<,,>)));

            // Need to mark where the group handler is injected so the scope decorator
            // can deocorate this handler and run requests generated by the grouper in parallel.
            // We cannot conditionally register the scope decorator because it has a Func<>
            // that creates new contexts only when registered as a decorator
            context.Container.RegisterConditional(
                serviceType,
                typeof(DummyScopedBatchRequestDelegator<,>),
                c => CanHandle(c)
                    && IsTypeDefinition(c.Consumer.ImplementationType, typeof(GroupedBatchRequestDecorator<,>)));

            // run this only once not within a macro request, only the requests
            // that are generated from the macro
            context.Container.RegisterConditional(
                serviceType,
                typeof(DeadlockRetryRequestDecorator<,>),
                c => CanHandle(c)
                    &&
                    (
                        IsTypeDefinition(c.Consumer.ImplementationType, typeof(AuthorizationRequestDecorator<,>))
                        || IsTypeDefinition(c.Consumer.ImplementationType, typeof(DummyScopedBatchRequestDelegator<,>))
                    )
                    && !c.ServiceType.GetGenericArguments()[0].IsMacro());

            context.Container.RegisterConditional(
                serviceType,
                typeof(TransactionRequestDecorator<,>),
                c => CanHandle(c)
                    && !c.ServiceType.GetGenericArguments()[0].IsQueryType()
                    && IsTypeDefinition(c.Consumer.ImplementationType, typeof(DeadlockRetryRequestDecorator<,>)));

            #region Event Pipeline Additions

            // decorate the inner most handler that is not a decorator running as a service
            // most of the time this would be the actual command handler
            context.Container.RegisterDecorator(
                serviceType,
                typeof(EventConsumingRequestDecorator<,>),
                c => !IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>))
                    && !IsTypeDefinition(c.ImplementationType, typeof(DeadlockRetryRequestDecorator<,>))
                    && !IsTypeDefinition(c.ImplementationType, typeof(TransactionRequestDecorator<,>))
                    && !IsTypeDefinition(c.ImplementationType, typeof(ValidationRequestDecorator<,>))
            );

            context.Container.RegisterDecorator(
                serviceType,
                typeof(AutomationRequestDecorator<,>),
                c => c.AppliedDecorators
                    .Where(a => a.IsGenericType)
                    .Any(a => a.GetGenericTypeDefinition() == typeof(EventConsumingRequestDecorator<,>)));

            #endregion

            #region Batch & Macro Additions

            context.Container.RegisterDecorator(
                serviceType,
                typeof(ValidationRequestDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(DeadlockRetryRequestDecorator<,>)));

            context.Container.RegisterDecorator(
                serviceType,
                typeof(SimpleInjectorScopedBatchRequestProxy<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>))
                    || IsTypeDefinition(c.ImplementationType, typeof(DummyScopedBatchRequestDelegator<,>)));

            // Apply this as a decorator for batch requests before the ScopeBatchRequestProxy
            // Need the conditional registration for macro requests
            context.Container.RegisterDecorator(
                serviceType,
                typeof(GroupedBatchRequestDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));

            // register this conditionally too so we can inject it at certain points
            // that need new validation not handled by the decoration registration
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

            // run this once on the outer most context
            context.Container.RegisterDecorator(
                serviceType,
                typeof(RequestExceptionDecorator<,>),
                c => IsTypeDefinition(c.ImplementationType, typeof(AuthorizationRequestDecorator<,>)));
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
            public bool AuthorizeObject(T instance) => true;
        }

        // Marker handler so the ScopedBatchRequestProxy can be registered
        // for macros
        private sealed class DummyScopedBatchRequestDelegator<T, R> : IRequestHandler<IEnumerable<T>, R>
            where T : notnull
        {
            private readonly IRequestHandler<IEnumerable<T>, R> inner;

            public DummyScopedBatchRequestDelegator(IRequestHandler<IEnumerable<T>, R> inner)
            {
                this.inner = inner.NotNull().Expect(nameof(inner));
            }

            public Task<Result<R, Exception>> HandleAsync(IEnumerable<T> request, CancellationToken cancelToken)
            {
                return inner.HandleAsync(request, cancelToken);
            }
        }
    }
}
