using System.Linq;
using BusinessApp.Infrastructure;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Registers authorization services
    /// </summary>
    public class AutomationRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public AutomationRegister(IBootstrapRegister inner) => this.inner = inner;

        public void Register(RegistrationContext context)
        {
            var container = context.Container;
            var serviceType = typeof(IRequestHandler<,>);

            container.Register<IProcessManager, SimpleInjectorProcessManager>();

            inner.Register(context);

            container.RegisterDecorator(
                serviceType,
                typeof(AutomationRequestDecorator<,>),
                c => c.AppliedDecorators
                    .Where(a => a.IsGenericType)
                    .Any(a => a.GetGenericTypeDefinition() == typeof(EventConsumingRequestDecorator<,>)));
        }
    }
}
