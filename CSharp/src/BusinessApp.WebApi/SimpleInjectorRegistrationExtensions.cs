namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Hosting;
    using SimpleInjector;

    public static class SimpleInjectorRegistrationExtensions
    {
        public static void RegisterCommandHandlers(this Container container, IEnumerable<Assembly> assemblies)
        {
            var flag = Environment.GetEnvironmentVariable("BATCH_PROCESSING");

            if (string.Compare(flag, "MULTI", true) == 0)
            {
                WrapService(container, assemblies);
            }
            else
            {
                WrapImplementation(container, assemblies);
            }
        }

        public static void RegisterLoggers(this Container container, IHostingEnvironment env)
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

        private static void WrapService(Container container, IEnumerable<Assembly> assemblies)
        {
            container.Register(typeof(ICommandHandler<>), assemblies);

            container.RegisterConditional(
                typeof(ICommandHandler<>),
                typeof(BatchCommandHandler<>),
                ctx => !ctx.Handled);
        }

        /// <summary>
        /// Registers concrete type and interface. When interface is injected you get the
        /// entire decoration graph. When concrete type is injected you just get the handler
        /// </summary>
        private static void WrapImplementation(Container container, IEnumerable<Assembly> assemblies)
        {

            var types = container.GetTypesToRegister(typeof(ICommandHandler<>), assemblies);

            foreach (var type in types)
            {
                container.Register(type);

                foreach (var i in type.GetInterfaces().Where(i => i.Name == typeof(ICommandHandler<>).Name))
                {
                    container.Register(
                        typeof(ICommandHandler<>).MakeGenericType(i.GetGenericArguments()[0]),
                        () => container.GetInstance(type));

                    // TODO validators
                    var genericBatchHandler = typeof(BatchCommandHandler<>)
                            .MakeGenericType(i.GetGenericArguments()[0]);

                    container.Register(
                        typeof(ICommandHandler<>).MakeGenericType(
                            typeof(IEnumerable<>).MakeGenericType(i.GetGenericArguments()[0])),
                        () => Activator.CreateInstance(
                            genericBatchHandler, new[] { container.GetInstance(type) }));
                }
            }
        }
    }
}
