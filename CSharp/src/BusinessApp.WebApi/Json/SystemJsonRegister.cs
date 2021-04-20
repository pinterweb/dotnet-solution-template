using BusinessApp.CompositionRoot;

namespace BusinessApp.WebApi.Json
{
    public class SystemJsonRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public SystemJsonRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.RegisterDecorator(typeof(IHttpRequestHandler<,>),
                typeof(SystemJsonExceptionDecorator<,>));

            inner.Register(context);
        }
    }
}
