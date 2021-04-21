using SimpleInjector;

namespace BusinessApp.WebApi.FunctionalTest
{
    internal interface IServiceConfiguration
    {
        void Configure(Container container);
    }
}
