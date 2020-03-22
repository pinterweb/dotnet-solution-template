namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Allows registering all types that are defined in the app layer
    /// </summary>
    public static class AppLayerBootstrapper
    {
        public static readonly Assembly Assembly = typeof(IQuery<>).Assembly;

        public static void Bootstrap(Container container, IHostingEnvironment env)
        {
            GuardAgainst.Null(container, nameof(container));

#if fluentvalidation
            container.Collection.Register(typeof(FluentValidation.IValidator<>), Assembly);
#endif
            container.Collection.Append(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
            container.Register(typeof(IValidator<>), typeof(CompositeValidator<>), Lifestyle.Singleton);

            container.Register(typeof(IQueryHandler<,>), Assembly);
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(AuthorizationQueryDecorator<,>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(EntityNotFoundQueryDecorator<,>));

            container.RegisterCommandHandlers(new[] { Assembly });

            container.Register<PostHandleRegister>();
            container.Register<IPostHandleRegister>(container.GetInstance<PostHandleRegister>);

            container.RegisterLoggers(env);

            // XXX Order of decorator registration matters.
            // Last registered runs first.
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationBatchCommandDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationCommandDecorator<>));

            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(TransactionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(DeadlockRetryDecorator<>));
        }
    }
}
