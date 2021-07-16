using BusinessApp.CompositionRoot;
using BusinessApp.Infrastructure.Persistence;
using BusinessApp.Kernel;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace BusinessApp.WebApi.IntegrationTest
{
    /// <summary>
    /// Extension to help container setup in tests
    /// </summary>
    public static class ContainerExtensions
    {
        public static Scope CreateScope(this Container container)
        {
            var config = A.Fake<IConfiguration>();
            var configSection = A.Fake<IConfigurationSection>();
            var _ = A.CallTo(() => config.GetSection("ConnectionStrings")).Returns(configSection);
            var __ = A.CallTo(() => configSection["Main"]).Returns("foo");
            var startup = new Startup(config, container, A.Dummy<IWebHostEnvironment>());

            return AsyncScopedLifestyle.BeginScope(container);
        }

        public static void CreateRegistrations(this Container container, string envName = "Development")
        {
            var bootstrapOptions = new RegistrationOptions(
                "Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar",
                envName)
            {
                RegistrationAssemblies = new[]
                {
                    typeof(ServiceRegistrationsTests).Assembly,
                    typeof(Infrastructure.IQuery).Assembly,
#if DEBUG
                    typeof(IQueryVisitor<>).Assembly,
#else
#if efcore
                    typeof(IQueryVisitor<>).Assembly,
#endif
#endif
                    typeof(IEventHandler<>).Assembly,
                    typeof(Startup).Assembly
                }
            };
            container.RegisterInstance(A.Fake<IHttpContextAccessor>());
            container.RegisterInstance(A.Fake<IStringLocalizerFactory>());

            Bootstrapper.RegisterServices(container,
                bootstrapOptions,
                A.Dummy<ILoggerFactory>());
        }

    }
}
