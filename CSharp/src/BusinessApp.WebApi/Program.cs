using System;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BusinessApp.Infrastructure;
using SimpleInjector;
using Microsoft.Extensions.Hosting;

namespace BusinessApp.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new ConsoleLogger(new StringLogEntryFormatter());

            try
            {
                logger.Log(new LogEntry(LogSeverity.Info,
                        $"Starting BusinessApp web host..."));

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Log(
                    new LogEntry(LogSeverity.Critical,
                        "BusinessApp terminated unexpectedly")
                    {
                        Exception = ex
                    });
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    _ = webBuilder.UseStartup<Startup>();
                });

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
