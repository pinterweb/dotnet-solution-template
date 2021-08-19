using BusinessApp.Infrastructure.Persistence;
using BusinessApp.Test.Shared;
using SimpleInjector;

namespace BusinessApp.WebApi.IntegrationTest
 {
    internal class EFCoreTestServiceConfiguration : IServiceConfiguration
    {
        public void Configure(Container container)
        {
            container.RegisterDecorator(
                typeof(BusinessAppDbContext),
                typeof(BusinessAppTestDbContext));
        }
    }
}
