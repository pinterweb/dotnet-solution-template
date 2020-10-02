namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
#if efcore
    using BusinessApp.Data;
#endif

    /// <summary>
    /// Allows registering all types that are defined in the app layer
    /// </summary>
    public static class AppLayerBootstrapper
    {
        public static readonly Assembly Assembly = typeof(IQuery<>).Assembly;

        public static void Bootstrap(Container container,
            IWebHostEnvironment env,
            BootstrapOptions options)
        {
            Guard.Against.Null(container).Expect(nameof(container));

            container.Collection.Register(typeof(IValidator<>), new[] { Assembly });

            container.Register(typeof(IBatchMacro<,>), Assembly);

#if fluentvalidation
            container.Collection.Register(typeof(FluentValidation.IValidator<>), Assembly);
            container.Collection.Append(typeof(IValidator<>), typeof(FluentValidationValidator<>));
#endif
            container.Register(typeof(IAuthorizer<>), typeof(AuthorizeAttributeHandler<>));

            container.Collection.Append(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
            container.Register(typeof(IValidator<>), typeof(CompositeValidator<>), Lifestyle.Singleton);

            container.Register(typeof(IBatchGrouper<>), Assembly);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);

            container.RegisterLoggers(env, options);

            var handlerTypes = container.GetTypesToRegister(typeof(IRequestHandler<,>),
                new[] { Assembly },
                new TypesToRegisterOptions
                {
                    IncludeGenericTypeDefinitions = true,
                    IncludeComposites = false,
                });

            foreach (var handlerType in handlerTypes) container.Register(handlerType);

            container.Register(typeof(IRequestHandler<,>), typeof(BatchCommandHandler<,>));
            container.Register(typeof(IRequestHandler<,>), typeof(BatchMacroCommandDecorator<,,>));

            // XXX Order of decorator registration matters.
            // First decorator wraps the real instance
            #region Decoration Registration
            container.RegisterQueryDecorator(typeof(QueryLifetimeCacheDecorator<,>));
#if efcore
            container.RegisterQueryDecorator(typeof(EFTrackingQueryDecorator<,>));
#endif
            container.RegisterQueryDecorator(typeof(EntityNotFoundQueryDecorator<,>));

            container.RegisterCommandDecorator(typeof(TransactionDecorator<,>),
                ctx => HasTransactionScope(ctx));

            container.RegisterCommandDecorator(typeof(DeadlockRetryDecorator<,>),
                ctx => HasTransactionScope(ctx));

            container.RegisterCommandDecorator(typeof(ApplicationScopeBatchDecorator<,>),
                lifestyle: Lifestyle.Singleton);

            container.RegisterCommandDecorator(typeof(BatchCommandGroupDecorator<,>));
            container.RegisterCommandDecorator(typeof(ValidationBatchCommandDecorator<,>));

            container.RegisterDecorator(typeof(IRequestHandler<,>),
                typeof(ValidationRequestDecorator<,>),
                ctx => ctx.ImplementationType != null &&
                    (
                        !ctx.ImplementationType.IsConstructedGenericType ||
                        ctx.ImplementationType.GetGenericTypeDefinition() != typeof(HandlerWrapper<,,>))
                    );

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(AuthorizationRequestDecorator<,>),
                c => c.ServiceType
                      .GetGenericArguments()[0]
                      .GetCustomAttributes(typeof(AuthorizeAttribute))
                      .Any()
                      && c.ImplementationType != null &&
                        (
                            !c.ImplementationType.IsConstructedGenericType ||
                            c.ImplementationType.GetGenericTypeDefinition() != typeof(HandlerWrapper<,,>)
                        ));

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(RequestExceptionDecorator<,>),
                ctx => ctx.ImplementationType != null &&
                    (
                        !ctx.ImplementationType.IsConstructedGenericType ||
                        ctx.ImplementationType.GetGenericTypeDefinition() != typeof(HandlerWrapper<,,>))
                    );

            #endregion

            container.RegisterConditional(typeof(IRequestHandler<,>),
                c =>
                {
                    var handler = handlerTypes.FirstOrDefault(t =>
                        t.GetInterfaces().Any(i => i == c.ServiceType));

                    var cmdType = c.ServiceType.GetGenericArguments()[0];

                    if (handler == null)
                    {
                        throw new BusinessAppWebApiException(
                            $"No command handler is setup for command '{cmdType.Name}'. Please set one up.");
                    }
                    else
                    {
                        return c.Consumer.ImplementationType.GetGenericTypeDefinition() == typeof(BatchCommandHandler<,>)
                            ? typeof(HandlerWrapper<,,>).MakeGenericType(handler, cmdType, cmdType)
                            : handler;
                    }
                },
                Lifestyle.Scoped,
                c => !c.Handled);
        }

        private static bool HasTransactionScope(DecoratorPredicateContext ctx)
        {
            return !ctx.ImplementationType.IsConstructedGenericType ||
                (
                    ctx.ImplementationType.GetGenericTypeDefinition() == typeof(BatchCommandHandler<,>) ||
                    (
                        ctx.ImplementationType.GetGenericTypeDefinition() != typeof(HandlerWrapper<,,>) &&
                        ctx.ImplementationType.GetGenericTypeDefinition() != typeof(BatchMacroCommandDecorator<,,>)
                    )
                );
        }

        private static void RegisterClosedAndOpenGenerics(this Container container, Type serviceType)
        {
            // we have to do this because by default, generic type definitions (such as the Constrained Notification Handler) won't be registered
            var handlerTypes = container.GetTypesToRegister(serviceType, new[] { Assembly }, new TypesToRegisterOptions
            {
                IncludeGenericTypeDefinitions = true,
                IncludeComposites = false,
            });

            container.Register(serviceType, handlerTypes);
        }

        public sealed class HandlerWrapper<TConsumer, TRequest, TResponse> :
            IRequestHandler<TRequest, TResponse>
            where TConsumer : IRequestHandler<TRequest, TResponse>
        {
            private readonly TConsumer inner;

            public HandlerWrapper(TConsumer inner)
            {
                this.inner = inner;
            }

            public Task<Result<TResponse, IFormattable>> HandleAsync(TRequest command,
                CancellationToken cancellationToken)
            {
                return inner.HandleAsync(command, cancellationToken);
            }
        }
    }
}
