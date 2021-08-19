using SimpleInjector;

namespace BusinessApp.WebApi.IntegrationTest
{
    internal interface IServiceConfiguration
    {
        void Configure(Container container);
    }
}
