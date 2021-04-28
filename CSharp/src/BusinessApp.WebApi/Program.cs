using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BusinessApp.Infrastructure;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new ConsoleLogger(new StringLogEntryFormatter());

            try
            {
                logger.Log(new LogEntry(LogSeverity.Info, $"Starting BusinessApp web host..."));
                var container = new Container();
                var builder = CreateContainerizedWebHostBuilder(args, container);

                builder.Build().Run();
            }
            catch (Exception ex)
            {
                logger.Log(new LogEntry(LogSeverity.Critical, "BusinessApp terminated unexpectedly")
                {
                    Exception = ex
                });
            }
        }

        public static IWebHostBuilder CreateContainerizedWebHostBuilder(string[] args, Container container) =>
            CreateWebHostBuilderCore(args).ConfigureServices(sc => sc.AddSingleton(container));

        // XXX needed for tests
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            CreateWebHostBuilderCore(args);

        private static IWebHostBuilder CreateWebHostBuilderCore(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    _ = logging.ClearProviders().AddConsole();
                })
                .ConfigureAppConfiguration(builder =>
                {
                    _ = builder.AddCommandLine(args).AddEnvironmentVariables();
                })
                .UseStartup<Startup>();
    }
}
