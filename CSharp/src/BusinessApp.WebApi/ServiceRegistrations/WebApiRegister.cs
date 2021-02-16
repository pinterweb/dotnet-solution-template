namespace BusinessApp.WebApi
{
    using System.Security.Principal;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using SimpleInjector;

    public class WebApiRegister : IBootstrapRegister
    {
        private readonly BootstrapOptions options;
        private readonly IBootstrapRegister inner;

        public WebApiRegister(BootstrapOptions options, IBootstrapRegister inner)
        {
            this.options = options;
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            inner.Register(context);

            container.RegisterSingleton<IPrincipal, HttpUserContext>();
            container.RegisterSingleton<IEventPublisher, SimpleInjectorEventPublisher>();
            container.RegisterSingleton<IAppScope, SimpleInjectorWebApiAppScope>();
            container.RegisterSingleton<IResponseWriter, HttpResponseWriter>();

            container.Register(typeof(IHttpRequestHandler<,>), options.RegistrationAssemblies);

            container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(EnvelopeQueryResourceHandler<,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestHandler<,>),
                ctx => !ctx.Handled);
        }
    }
}
