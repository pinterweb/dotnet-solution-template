namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System;
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

            container.Register(typeof(ICommandHandler<>), Assembly);
            container.Register<PostHandleRegister>();
            container.Register<IPostHandleRegister>(container.GetInstance<PostHandleRegister>);

            RegisterLoggers(container, env);

            // XXX Order of decorator registration matters.
            // Last registered runs first.
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(TransactionDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationBatchCommandDecorator<>));
            container.RegisterDecorator(typeof(ICommandHandler<>),
                typeof(ValidationCommandDecorator<>));
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

        private static void RegisterLoggers(Container container, IHostingEnvironment env)
        {
            container.Register(typeof(ILogger), typeof(CompositeLogger), Lifestyle.Singleton);

            if (env.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                container.Collection.Append<ILogger, TraceLogger>();
            }

            container.RegisterSingleton<ILogEntryFormatter, SerializedLogEntryFormatter>();
            container.RegisterInstance<IFileProperties>(new RollingFileProperties
            {
                Name = Environment.GetEnvironmentVariable("BUSINESSAPP_LOG_FILE") ??
                    $"{env.ContentRootPath}/app.log"
            });

            container.RegisterSingleton<ConsoleLogger>();
            container.RegisterSingleton<FileLogger>();

            container.Collection.Append<ILogger>(() =>
                new BackgroundLogDecorator(
                    container.GetInstance<FileLogger>(),
                    container.GetInstance<ConsoleLogger>()),
                Lifestyle.Singleton);
        }
    }
}
