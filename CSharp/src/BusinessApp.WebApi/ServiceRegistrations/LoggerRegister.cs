namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using Microsoft.AspNetCore.Hosting;
    using SimpleInjector;

    public class LoggerRegister : IBootstrapRegister
    {
        private readonly IWebHostEnvironment env;
        private readonly BootstrapOptions options;
        private readonly IBootstrapRegister inner;

        public LoggerRegister(IWebHostEnvironment env,
            BootstrapOptions options,
            IBootstrapRegister inner)
        {
            this.env = env;
            this.options = options;
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

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

            inner.Register(context);
        }
    }
}
