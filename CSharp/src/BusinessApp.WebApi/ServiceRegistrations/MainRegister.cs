using System.Security.Principal;
using BusinessApp.CompositionRoot;
using BusinessApp.WebApi.ProblemDetails;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Main web api service register
    /// </summary>
    public class MainRegister : IBootstrapRegister
    {
        private readonly RegistrationOptions options;
        private readonly IBootstrapRegister inner;

        public MainRegister(IBootstrapRegister inner, RegistrationOptions options)
        {
            this.inner = inner;
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            RegisterWebApiErrorResponse(context.Container);
            RegisterWebApiServices(context);

            inner.Register(context);
        }

        private void RegisterWebApiServices(RegistrationContext context)
        {
            context.Container.RegisterSingleton<IPrincipal, HttpUserContext>();
            context.Container.Register(typeof(IHttpRequestHandler<,>),
                options.RegistrationAssemblies);

            context.Container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(EnvelopeQueryResourceHandler<,>),
                ctx => !ctx.Handled);

            context.Container
                .RegisterSingleton<IHttpRequestHandler, SimpleInjectorHttpRequestHandler>();

            context.Container.RegisterConditional(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestHandler<,>),
                ctx => !ctx.Handled);

#if DEBUG
            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler),
                typeof(HttpRequestBodyAnalyzer),
                Lifestyle.Singleton
            );
#elif hasbatch
            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler),
                typeof(HttpRequestBodyAnalyzer),
                Lifestyle.Singleton
            );
#endif

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler),
                typeof(HttpRequestLoggingDecorator),
                Lifestyle.Singleton
            );

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpRequestLoggingDecorator<,>));

            context.Container.RegisterDecorator(
                typeof(IHttpRequestHandler<,>),
                typeof(HttpResponseDecorator<,>));
        }

        private static void RegisterWebApiErrorResponse(Container container)
        {
            container.RegisterInstance(ProblemDetailOptionBootstrap.KnownProblems);
            container.RegisterSingleton<IProblemDetailFactory, ProblemDetailFactory>();
            container.RegisterDecorator<IProblemDetailFactory, LocalizedProblemDetailFactory>();
        }
    }
}
