using System;
using BusinessApp.Infrastructure;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace BusinessApp.WebApi.IntegrationTest
{
    public class AuthorizationRegistrationTests : IDisposable
    {
        private readonly Container container;
        private readonly Scope scope;
        private readonly IConfiguration config;

        public AuthorizationRegistrationTests()
        {
            config = (IConfiguration)Program.CreateWebHostBuilder(new string[0])
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder.AddJsonFile("appsettings.test.json");
                    builder.AddEnvironmentVariables(prefix: "PurchaseOrderItemReceiver_");
                })
                .UseStartup<Startup>()
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            container = new Container();
            scope = container.CreateScope();
        }

        public void Dispose() => scope.Dispose();

        [Fact]
        public void IAuthorization_WithAuthAttribute_ReturnsAuthorizationAttributeHandler()
        {
            /* Arrange */
            var requestType = typeof(AuthWithAttrubte);
            container.CreateRegistrations(config);
            container.Verify();
            var serviceType = typeof(IAuthorizer<>).MakeGenericType(requestType);

            /* Act */
            var instance = container.GetInstance(serviceType);

            /* Assert */
            Assert.Equal(typeof(AuthorizeAttributeHandler<AuthWithAttrubte>), instance.GetType());
        }

        [Fact]
        public void IAuthorization_WithoutAuthAttribute_ReturnsNullAuthorizer()
        {
            /* Arrange */
            var requestType = typeof(AuthWithOutAttrubte);
            container.CreateRegistrations(config);
            container.Verify();
            var serviceType = typeof(IAuthorizer<>).MakeGenericType(requestType);

            /* Act */
            var instance = container.GetInstance(serviceType);

            /* Assert - assert name because it is private */
            Assert.StartsWith("NullAuthorizer", instance.GetType().Name);
        }

        [Authorize]
        class AuthWithAttrubte {}
        class AuthWithOutAttrubte {}
    }
}
