using BusinessApp.Infrastructure;
using BusinessApp.CompositionRoot;
using BusinessApp.Kernel;
#if DEBUG
using BusinessApp.Infrastructure.Persistence;
#elif efcore
using BusinessApp.Infrastructure.Persistence;
#endif
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BusinessApp.WebApi.IntegrationTest
{
    /// <summary>
    /// Extension to help container setup in tests
    /// </summary>
    public static class ContainerExtensions
    {
        public static void CreateTestApp(this Container container,
            string environmentName = "Development")
        {
            var config = (IConfiguration)Program.CreateHostBuilder(new string[0])
                .ConfigureAppConfiguration((hostBuilder, configBuilder) =>
                {
                    _ = configBuilder
                        .AddJsonFile("appsettings.test.json")
                        .AddEnvironmentVariables(prefix: "BusinessApp_");
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            container.CreateRegistrations(config, environmentName);
        }

        public static void CreateRegistrations(this Container container, IConfiguration config,
            string envName = "Development")
        {
            var bootstrapOptions = new RegistrationOptions(
                "Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar",
                envName)
            {
                RegistrationAssemblies = new[]
                {
                    typeof(ContainerExtensions).Assembly,
                    typeof(ConsoleLogger).Assembly,
#if DEBUG
                    typeof(IQueryVisitor<>).Assembly,
#elif efcore
                    typeof(IQueryVisitor<>).Assembly,
#endif
                    typeof(ValueKind).Assembly,
                    typeof(Startup).Assembly
                }
            };
            container.RegisterInstance(A.Fake<IHttpContextAccessor>());
            container.RegisterInstance(A.Fake<IStringLocalizerFactory>());

            Bootstrapper.RegisterServices(container,
                bootstrapOptions,
                new Dictionary<Type, object>
                {
                    { typeof(ILoggerFactory), A.Dummy<ILoggerFactory>() },
                    { typeof(IConfiguration), config },

                });
        }

        public static IEnumerable<Type> GetServiceGraph(this Container container,
            params Type[] serviceTypes)
        {
            var firstType = container.GetRegistration(serviceTypes.First());

            return firstType
                .GetDependencies()
                .Where(i => serviceTypes.Any(st => st.IsAssignableFrom(i.ServiceType)))
                .Prepend(firstType)
                .Select(ip => ip.Registration.ImplementationType)
                .Where(t => t.IsVisible);
        }
    }
}
