namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using BusinessApp.App;

    public class ValidationRegister : IBootstrapRegister
    {
        private readonly BootstrapOptions options;
        private readonly IBootstrapRegister inner;

        public ValidationRegister(BootstrapOptions options, IBootstrapRegister inner)
        {
            this.options = options;
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            inner.Register(context);

            var validatorTypes = container.GetTypesToRegister(
                typeof(IValidator<>),
                options.RegistrationAssemblies
            );

            foreach (var type in validatorTypes)
            {
                container.Collection.Append(typeof(IValidator<>), type);
            }

            container.Register(typeof(IValidator<>),
                typeof(CompositeValidator<>),
                Lifestyle.Singleton);
        }
    }
}
