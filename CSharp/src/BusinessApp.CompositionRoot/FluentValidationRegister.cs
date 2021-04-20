using BusinessApp.Infrastructure;

namespace BusinessApp.CompositionRoot
{
    public class FluentValidationRegister : IBootstrapRegister
    {
        private readonly RegistrationOptions options;
        private readonly IBootstrapRegister inner;

        public FluentValidationRegister(RegistrationOptions options, IBootstrapRegister inner)
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
