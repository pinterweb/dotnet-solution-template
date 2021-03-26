namespace BusinessApp.WebApi
{
    using BusinessApp.Domain;
    using SimpleInjector;

    public class RegistrationContext
    {
        public RegistrationContext(Container container)
        {
            Container = container.NotNull().Expect(nameof(container));
        }

        public Container Container { get; }
    }
}
