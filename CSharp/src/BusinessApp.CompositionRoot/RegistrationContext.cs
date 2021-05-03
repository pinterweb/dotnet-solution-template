using BusinessApp.Kernel;
using SimpleInjector;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Data context to hold data used to register services using the
    /// <see cref="IBootstrapRegister" />
    /// </summary>
    public class RegistrationContext
    {
        public RegistrationContext(Container container)
            => Container = container.NotNull().Expect(nameof(container));

        public Container Container { get; }
    }
}
