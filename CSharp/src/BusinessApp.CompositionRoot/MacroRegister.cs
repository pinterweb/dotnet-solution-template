using System;
using System.Linq;
using BusinessApp.Infrastructure;

namespace BusinessApp.CompositionRoot
{
    public static partial class RegisterExtensions
    {
        public static bool IsMacro(this Type type)
            => type
                .GetInterfaces()
                .Any(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IMacro<>));
    }

    /// <summary>
    /// Registers null services that may be removed based on the template install flags
    /// </summary>
    public class MacroRegister : IBootstrapRegister
    {
        private readonly RegistrationOptions options;
        private readonly IBootstrapRegister inner;

        public MacroRegister(RegistrationOptions options, IBootstrapRegister inner)
        {
            this.inner = inner;
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;
            container.Register(typeof(IBatchMacro<,>), options.RegistrationAssemblies);

            inner.Register(context);
        }
    }
}
