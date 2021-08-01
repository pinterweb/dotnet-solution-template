using System;
using System.Linq;
using BusinessApp.Infrastructure;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Registers null services that may be removed based on the template install flags
    /// </summary>
    public class BatchRegister : IBootstrapRegister
    {
        private readonly RegistrationOptions options;
        private readonly IBootstrapRegister inner;

        public BatchRegister(RegistrationOptions options, IBootstrapRegister inner)
        {
            this.inner = inner;
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.Register(typeof(IBatchGrouper<>), options.RegistrationAssemblies);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);

            inner.Register(context);
        }
    }
}
