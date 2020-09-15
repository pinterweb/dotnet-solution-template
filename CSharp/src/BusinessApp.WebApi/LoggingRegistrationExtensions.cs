namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Hosting;
    using SimpleInjector;

    public static partial class SimpleInjectorRegistrationExtensions
    {
        public static void RegisterLoggers(this Container container,
            IWebHostEnvironment env,
            BootstrapOptions options)
        {
            container.Register(typeof(ILogger), typeof(CompositeLogger), Lifestyle.Singleton);

            if (env.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                container.Collection.Append<ILogger, TraceLogger>();
            }

            container.RegisterSingleton<ILogEntryFormatter, SerializedLogEntryFormatter>();
            container.RegisterInstance<IFileProperties>(new RollingFileProperties
            {
                Name = options.LogFilePath ?? $"{env.ContentRootPath}/app.log"
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
