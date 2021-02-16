namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using BusinessApp.App;

    public class FluentValidationRegister : IBootstrapRegister
    {
        private readonly BootstrapOptions options;
        private readonly IBootstrapRegister inner;

        public FluentValidationRegister(BootstrapOptions options,
            IBootstrapRegister inner
        )
        {
            this.options = options;
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            inner.Register(context);

            container.Collection.Register(
                typeof(FluentValidation.IValidator<>),
                options.RegistrationAssemblies);

            container.Collection.Append(
                typeof(IValidator<>),
                typeof(FluentValidationValidator<>));

        }
    }
}
