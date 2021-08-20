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
using SimpleInjector;

namespace BusinessApp.WebApi.IntegrationTest
{
    /// <summary>
    /// Extension to help container setup in tests
    /// </summary>
    public static class ContainerExtensions
    {
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
                A.Dummy<ILoggerFactory>(),
                config);
        }
    }
}
