using BusinessApp.CompositionRoot;

namespace BusinessApp.Infrastructure.WebApi.Json
{
    public class NewtonsoftJsonRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public NewtonsoftJsonRegister(IBootstrapRegister inner) => this.inner = inner;

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.RegisterDecorator(typeof(IHttpRequestHandler<,>),
                typeof(NewtonsoftJsonExceptionDecorator<,>));

            inner.Register(context);
        }
    }
}
