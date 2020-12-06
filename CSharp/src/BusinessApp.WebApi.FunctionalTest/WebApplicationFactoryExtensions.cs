namespace BusinessApp.WebApi.FunctionalTest
{
    using System;
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

    public static class WebApplicationFactoryExtensions
    {
        private const string AuthenticationScheme = "FakeAuthenticationScheme";

        public static HttpClient NewClient<T>(this WebApplicationFactory<T> factory)
            where T : class
        {
            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.test.json");
                })
                .ConfigureTestServices(services =>
                {
                    services.AddAuthentication(AuthenticationScheme)
                            .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>
                            (AuthenticationScheme, options => { });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationScheme);

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
            {
            }

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
