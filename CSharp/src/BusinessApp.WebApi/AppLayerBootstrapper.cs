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
            GuardAgainst.Null(container, nameof(container));

            container.Collection.Register(typeof(IValidator<>), new[] { Assembly });

#if fluentvalidation
            container.Collection.Register(typeof(FluentValidation.IValidator<>), Assembly);
            container.Collection.Append(typeof(IValidator<>), typeof(FluentValidationValidator<>));
#endif
            container.Register(typeof(IAuthorizer<>), typeof(AuthorizeAttributeHandler<>));
            container.Collection.Append(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
            container.Register(typeof(IValidator<>), typeof(CompositeValidator<>), Lifestyle.Singleton);

            container.Register(typeof(IQueryHandler<,>), Assembly);
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(EntityNotFoundQueryDecorator<,>));

            container.Register(typeof(IBatchGrouper<>), Assembly);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);

            var handlerTypes = container.GetTypesToRegister(typeof(ICommandHandler<>), Assembly);

            foreach (var type in handlerTypes) container.Register(type);

            container.Register(typeof(ICommandHandler<>), typeof(BatchCommandHandler<>));

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
                    var cmd = handler.GetInterfaces().First().GetGenericArguments()[0];

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
                (ctx.ImplementationType.GetGenericTypeDefinition() == typeof(BatchCommandHandler<>) ||
                ctx.ImplementationType.GetGenericTypeDefinition() != typeof(HandlerWrapper<,>));
        }

        public sealed class HandlerWrapper<TConsumer, T> : ICommandHandler<T>
            where TConsumer : ICommandHandler<T>
        {
            private readonly TConsumer consumer;

            public HandlerWrapper(TConsumer consumer)
            {
                this.consumer = consumer;

            }

            public Task HandleAsync(T command, CancellationToken cancellationToken)
            {
                return consumer.HandleAsync(command, cancellationToken);
            }
        }
    }
}
