namespace BusinessApp.WebApi
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
#if DEBUG
    using Microsoft.AspNetCore.Server.HttpSys;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
#endif

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateWebHostBuilder(args);

#if DEBUG
            if(string.Compare(Process.GetCurrentProcess().ProcessName, "iisexpress") != 0)
            {
                builder.UseHttpSys(opt =>
                {
                    opt.Authentication.Schemes =
                        AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                    opt.Authentication.AllowAnonymous = true;
                });
            }
#endif

            builder.Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentVariables();
                })
#if DEBUG
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                })
#endif
                .UseStartup<Startup>();
    }
}
