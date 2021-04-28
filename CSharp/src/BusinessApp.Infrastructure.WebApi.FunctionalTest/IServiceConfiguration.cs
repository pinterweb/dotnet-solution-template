using SimpleInjector;

namespace BusinessApp.Infrastructure.WebApi.FunctionalTest
{
    internal interface IServiceConfiguration
    {
        void Configure(Container container);
    }
}
