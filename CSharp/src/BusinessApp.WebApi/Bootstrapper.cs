namespace BusinessApp.WebApi
{
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using SimpleInjector;

    public static partial class Bootstrapper
    {
        public static void RegisterServices(Container container,
            BootstrapOptions options,
            IWebHostEnvironment env)
        {
            var bootstrapContainer = SetupBootstrapContainer(options, env);

            RegisterBootstrapDecorators(bootstrapContainer);

            bootstrapContainer.Verify();

            var bootstrap = bootstrapContainer.GetInstance<IBootstrapRegister>();
            var regContext = new RegistrationContext { Container = container };

            bootstrap.Register(regContext);

            // XXX make sure this is absolultey last
            container.RegisterDecorator(typeof(IHttpRequestHandler<,>),
                typeof(HttpResponseDecorator<,>));

        }

        private static Container SetupBootstrapContainer(BootstrapOptions options,
            IWebHostEnvironment env)
        {
            var bootstrapContainer = new Container();

            bootstrapContainer.Register<IBootstrapRegister, MainRegister>();
            bootstrapContainer.RegisterInstance(options);
            bootstrapContainer.RegisterInstance(env);

            return bootstrapContainer;
        }

        private static void RegisterBootstrapDecorators(Container container)
        {
            var bootstrapDecorators = container
                .GetTypesToRegister(typeof(IBootstrapRegister),
                    new[] { typeof(Startup).Assembly },
                    new TypesToRegisterOptions
                    {
                        IncludeDecorators = true
                    })
                .Where(type => type != typeof(MainRegister));

            foreach (var type in bootstrapDecorators)
            {
                container.RegisterDecorator(typeof(IBootstrapRegister),type);
            }
        }

        private static void RegisterServices()
        {
        }
    }
 }
