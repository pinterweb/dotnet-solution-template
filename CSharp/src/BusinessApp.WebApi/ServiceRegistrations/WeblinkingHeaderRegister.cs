using BusinessApp.CompositionRoot;

namespace BusinessApp.WebApi.ServiceRegistrations
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
#if DEBUG
            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(WeblinkingHeaderEventRequestDecorator<,>)
            );
#elif events
            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(WeblinkingHeaderEventRequestDecorator<,>)
            );
#endif

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(WeblinkingHeaderRequestDecorator<,>)
            );

#if DEBUG
            context.Container.Collection.Register(
                typeof(HateoasEventLink<,>),
                options.RegistrationAssemblies);
#elif events
            context.Container.Collection.Register(
                typeof(HateoasEventLink<,>),
                options.RegistrationAssemblies);
#endif

            context.Container.Collection.Register(
                typeof(HateoasLink<,>),
                options.RegistrationAssemblies);

            inner.Register(context);
        }
    }
}
