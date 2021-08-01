using BusinessApp.Infrastructure;
using BusinessApp.Kernel;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Registers null services that may be removed based on the template install flags
    /// </summary>
    public class EventRegister : IBootstrapRegister
    {
        private readonly RegistrationOptions options;
        private readonly IBootstrapRegister inner;

        public EventRegister(RegistrationOptions options, IBootstrapRegister inner)
        {
            this.inner = inner;
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.Collection.Register(typeof(IEventHandler<>), options.RegistrationAssemblies);
            container.Register<IEventPublisherFactory, EventMetadataPublisherFactory>();
            context.Container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();

            inner.Register(context);
        }
    }
}
