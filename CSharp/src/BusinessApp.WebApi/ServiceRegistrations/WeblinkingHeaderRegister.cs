namespace BusinessApp.WebApi
{
    /// <summary>
    /// Registers services to support HATEOAS as defined in RFC8288
    /// </summary>
    public class WeblinkingHeaderRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public WeblinkingHeaderRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(WeblinkingHeaderEventRequestDecorator<,>)
            );

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(WeblinkingHeaderRequestDecorator<,>)
            );

            inner.Register(context);
        }
    }
}
