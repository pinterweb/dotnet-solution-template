namespace BusinessApp.WebApi
{
    using BusinessApp.App;

    public class BatchRequestRegister : IBootstrapRegister
    {
        private readonly BootstrapOptions options;
        private readonly IBootstrapRegister inner;

        public BatchRequestRegister(BootstrapOptions options,
            IBootstrapRegister inner)
        {
            this.options = options;
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.Register(typeof(IBatchMacro<,>), options.RegistrationAssemblies);

            container.Register(typeof(IBatchGrouper<>), options.RegistrationAssemblies);

            inner.Register(context);

            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);

        }
    }
}
