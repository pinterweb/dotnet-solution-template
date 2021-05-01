using BusinessApp.CompositionRoot;

namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Registers services to support HATEOAS as defined in RFC8288
    /// </summary>
    public class WeblinkingHeaderRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;
        private readonly RegistrationOptions options;

        public WeblinkingHeaderRegister(IBootstrapRegister inner, RegistrationOptions options)
        {
            this.inner = inner;
            this.options = options;
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

            context.Container.Collection.Register(
                typeof(HateoasEventLink<,>),
                options.RegistrationAssemblies);

            context.Container.Collection.Register(
                typeof(HateoasLink<,>),
                options.RegistrationAssemblies);

            inner.Register(context);
        }
    }
}
