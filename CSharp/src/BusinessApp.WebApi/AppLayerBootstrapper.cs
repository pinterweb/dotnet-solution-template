namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;

    /// <summary>
    /// Allows registering all types that are defined in the app layer
    /// </summary>
    public static class AppLayerBootstrapper
    {
        public static readonly Assembly Assembly = typeof(IQuery<>).Assembly;

        public static void Bootstrap(Container container)
        {
            GuardAgainst.Null(container, nameof(container));

#if fluentvalidation
            container.Collection.Register(typeof(FluentValidation.IValidator<>), Assembly);
#endif
            container.Register(typeof(IAuthorizer<>), typeof(AuthorizeAttributeHandler<>));
            container.Collection.Append(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
            container.Register(typeof(IValidator<>), typeof(CompositeValidator<>), Lifestyle.Singleton);

            container.Register(typeof(IQueryHandler<,>), Assembly);
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(AuthorizationQueryDecorator<,>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(EntityNotFoundQueryDecorator<,>));

            container.Register(typeof(ICommandHandler<>), Assembly);
            container.Register<PostHandleRegister>();
            container.Register<IPostHandleRegister>(container.GetInstance<PostHandleRegister>);

            // XXX Order of decorator registration matters.
            // Last registered runs first.
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(TransactionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationBatchCommandDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationCommandDecorator<>));
            container.RegisterDecorator(
                typeof(ICommandHandler<>),
                typeof(AuthorizationCommandDecorator<>)
            );
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(DeadlockRetryDecorator<>));
            // The batch decorator should wrap
            // the real instance so this all happens in one transaction!
            // batch command should be in their own separate transaction
            container.RegisterConditional(
                typeof(ICommandHandler<>),
                typeof(BatchCommandDecorator<>),
                ctx => !ctx.Handled);
        }
    }
}
