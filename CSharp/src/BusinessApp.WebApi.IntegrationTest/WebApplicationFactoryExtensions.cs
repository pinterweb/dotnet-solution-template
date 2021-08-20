using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleInjector;

namespace BusinessApp.WebApi.IntegrationTest
{
    public static class WebApplicationFactoryExtensions
    {
        private const string AuthenticationScheme = "FakeAuthenticationScheme";

        public static HttpClient NewClient<T>(this WebApplicationFactory<T> factory,
            Action<Container> configuringServices = null)
            where T : class
        {
            var startupContainer = new Container();
            startupContainer.Collection.Register<IServiceConfiguration>(new[]
            {
                typeof(WebApplicationFactoryExtensions).Assembly
            });
            var client = factory
                .WithWebHostBuilder(builder =>
                {
                    _ = builder.ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        _ = config.AddJsonFile("appsettings.test.json")
                            .AddEnvironmentVariables(prefix: "BusinessApp_Test_");
                    })
                    .ConfigureTestServices(services =>
                    {
                        var container = (Container)services
                            .First(s => s.ServiceType == typeof(Container))
                            .ImplementationInstance;

                        configuringServices?.Invoke(container);

                        _ = services.AddAuthentication(AuthenticationScheme)
                                .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>
                                (AuthenticationScheme, options => { });

                        foreach (var configSvc in startupContainer.GetAllInstances<IServiceConfiguration>())
                        {
                            configSvc.Configure(container);
                        }
                    });
                })
                .CreateClient(new WebApplicationFactoryClientOptions());

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                AuthenticationScheme);

            return client;
        }

        public class FakeAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public FakeAuthenticationHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder
                encoder,
                ISystemClock clock) : base(options, logger, encoder, clock)
            { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "FakeUserId"),
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                };
                var identity = new ClaimsIdentity(claims, "Fake");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Fake");

                var result = AuthenticateResult.Success(ticket);

                return Task.FromResult(result);
            }
        }
    }
}
