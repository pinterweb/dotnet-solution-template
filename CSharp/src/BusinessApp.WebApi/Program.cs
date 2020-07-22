namespace BusinessApp.WebApi
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Server.HttpSys;
    using Microsoft.Extensions.Configuration;
    using BusinessApp.App;
    using BusinessApp.Domain;

    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new ConsoleLogger(new StringLogEntryFormatter());

            try
            {
                logger.Log(new LogEntry(LogSeverity.Info, $"Starting BusinessApp web host..."));
                var builder = CreateWebHostBuilder(args);

                if(string.Compare(Process.GetCurrentProcess().ProcessName, "iisexpress") != 0)
                {
                    logger.Log(new LogEntry(LogSeverity.Info, $"Using httpsys options..."));
                    builder.UseHttpSys(opt =>
                    {
                        opt.Authentication.Schemes =
                            AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                        opt.Authentication.AllowAnonymous = true;
                    });
                }

                builder.Build().Run();
            }
            catch (Exception ex)
            {
                logger.Log(
                    new LogEntry(LogSeverity.Critical,
                        $"BusinessApp terminated unexpectedly",
                        ex));
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentVariables();
                })
                .UseStartup<Startup>();
    }
}
