using BusinessApp.App;

namespace BusinessApp.WebApi
{
    public class DataAnnotationRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public DataAnnotationRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.Collection.Register(
                typeof(IValidator<>),
                new[] { typeof(DataAnnotationsValidator<>) });

            inner.Register(context);
        }
    }
}
