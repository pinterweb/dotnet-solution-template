using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BusinessApp.Infrastructure;
using SimpleInjector;
using System.Linq;

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
            CreateWebHostBuilderCore(args).ConfigureServices(sc => sc.AddSingleton(new Container()));

        private static IWebHostBuilder CreateWebHostBuilderCore(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    _ = logging.ClearProviders().AddConsole();
                })
                .ConfigureAppConfiguration(builder =>
                {
                    _ = builder
                        .AddCommandLine(args)
                        .AddEnvironmentVariables(prefix: "BusinessApp_");

                    ReplaceJsonConfigProvider(builder);
                })
                .UseStartup<Startup>();

        private static void ReplaceJsonConfigProvider(IConfigurationBuilder builder)
        {
            var jsonConfigurationSources = builder.Sources.OfType<JsonConfigurationSource>().ToList();

            foreach (var src in jsonConfigurationSources)
            {
                var indexOfJsonConfigurationSource = builder.Sources
                    .IndexOf(src);

                builder.Sources.RemoveAt(indexOfJsonConfigurationSource);
                builder.Sources.Insert(
                    indexOfJsonConfigurationSource,
                    new ExpandJsonConfigurationSource
                    {
                        FileProvider = src.FileProvider,
                        Path = src.Path,
                        Optional = src.Optional,
                        ReloadOnChange = src.ReloadOnChange
                    });
            }
        }
    }
}
