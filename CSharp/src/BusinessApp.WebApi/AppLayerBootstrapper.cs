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

#if fluentvalidation
            container.Collection.Register(typeof(FluentValidation.IValidator<>), Assembly);
            container.Collection.Append(typeof(IValidator<>), typeof(FluentValidationValidator<>));
#endif
            container.Register(typeof(IAuthorizer<>), typeof(AuthorizeAttributeHandler<>));
            container.Collection.Append(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
            container.Register(typeof(IValidator<>), typeof(CompositeValidator<>), Lifestyle.Singleton);

            container.Register(typeof(IQueryHandler<,>), Assembly);
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(ValidationQueryDecorator<,>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(QueryLifetimeCacheDecorator<,>));
#if efcore
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(EFTrackingQueryDecorator<,>));
#endif
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(EntityNotFoundQueryDecorator<,>));
            container.RegisterDecorator(
                typeof(IQueryHandler<,>),
                typeof(AuthorizationQueryDecorator<,>),
                c => c.ServiceType
                      .GetGenericArguments()[0]
                      .GetCustomAttributes(typeof(AuthorizeAttribute))
                      .Any());

            container.Register(typeof(IBatchGrouper<>), Assembly);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);

            container.RegisterLoggers(env, options);

            var handlerTypes = container.GetTypesToRegister(typeof(ICommandHandler<>), Assembly);

            foreach (var type in handlerTypes) container.Register(type);

            container.Register(typeof(ICommandHandler<>), typeof(BatchCommandHandler<>));
            container.Register(typeof(ICommandHandler<>), typeof(BatchMacroCommandDecorator<,>));

            // XXX Order of decorator registration matters.
            // First decorator wraps the real instance
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(TransactionDecorator<>),
                ctx => HasTransactionScope(ctx));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(DeadlockRetryDecorator<>),
                ctx => HasTransactionScope(ctx));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ApplicationScopeBatchDecorator<>),
                Lifestyle.Singleton);

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(BatchCommandGroupDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationBatchCommandDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationCommandDecorator<>),
                ctx => !ctx.ImplementationType.IsConstructedGenericType ||
                    ctx.ImplementationType.GetGenericTypeDefinition() != typeof(HandlerWrapper<,>));

            container.RegisterConditional(typeof(ICommandHandler<>),
                c =>
                {
                    var handler = handlerTypes.First(t => t.GetInterfaces().Any(i => i == c.ServiceType));
                    var cmd = c.ServiceType.GetGenericArguments()[0];

                    return c.Consumer.ImplementationType.GetGenericTypeDefinition() == typeof(BatchCommandHandler<>)
                        ? typeof(HandlerWrapper<,>).MakeGenericType(handler, cmd)
                        : handler;
                },
                Lifestyle.Scoped,
                c => !c.Handled);
        }

        private static bool HasTransactionScope(DecoratorPredicateContext ctx)
        {
            return !ctx.ImplementationType.IsConstructedGenericType ||
                (
                    ctx.ImplementationType.GetGenericTypeDefinition() == typeof(BatchCommandHandler<>) ||
                    (
                        ctx.ImplementationType.GetGenericTypeDefinition() != typeof(HandlerWrapper<,>) && 
                        ctx.ImplementationType.GetGenericTypeDefinition() != typeof(BatchMacroCommandDecorator<,>)
                    )
                );
        }

        public sealed class HandlerWrapper<TConsumer, T> : ICommandHandler<T>
            where TConsumer : ICommandHandler<T>
        {
            private readonly TConsumer inner;

            public HandlerWrapper(TConsumer inner)
            {
                this.inner = inner;

            }

            public Task HandleAsync(T command, CancellationToken cancellationToken)
            {
                return inner.HandleAsync(command, cancellationToken);
            }
        }
    }
}
