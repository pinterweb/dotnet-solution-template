namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Hosting;
    using System;

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
            container.Collection.Append(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
            container.Register(typeof(IValidator<>), typeof(CompositeValidator<>), Lifestyle.Singleton);

            container.Register(typeof(IQueryHandler<,>), Assembly);
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(AuthorizationQueryDecorator<,>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(EntityNotFoundQueryDecorator<,>));

            container.Register(typeof(IBatchGrouper<>), Assembly);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);

            var handlerTypes = container.GetTypesToRegister(typeof(ICommandHandler<>), Assembly);
            container.RegisterCommandHandlersInOneBatch(handlerTypes);

            container.RegisterLoggers(env, options);

            // XXX Order of decorator registration matters.
            // First decorator wraps the real instance
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(TransactionDecorator<>),
                ctx => IsNotBatchHandler(ctx.ImplementationType));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationBatchCommandDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationCommandDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(DeadlockRetryDecorator<>),
                ctx => IsNotBatchHandler(ctx.ImplementationType));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(SimpleInjectorAsyncScopeCommandProxy<>),
                Lifestyle.Singleton);

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(BatchCommandGroupDecorator<>));
        }

        private static bool IsNotBatchHandler(Type implementationType)
        {
            return !implementationType.IsConstructedGenericType ||
                implementationType.GetGenericTypeDefinition() != typeof(BatchCommandHandler<>);
        }
    }
}
