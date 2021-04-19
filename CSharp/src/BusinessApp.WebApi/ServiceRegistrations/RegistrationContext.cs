using BusinessApp.Domain;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    public class RegistrationContext
    {
        public RegistrationContext(Container container)
        {
            Container = container.NotNull().Expect(nameof(container));
        }

        public Container Container { get; }
    }
}
