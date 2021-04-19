using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace BusinessApp.WebApi
{
    public static partial class Bootstrapper
    {
        public static void RegisterServices(Container container,
            BootstrapOptions options,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory)
        {
            var bootstrapContainer = SetupBootstrapContainer(options, env, loggerFactory);

            RegisterBootstrapDecorators(bootstrapContainer);

            bootstrapContainer.Verify();

            var bootstrap = bootstrapContainer.GetInstance<IBootstrapRegister>();
            var regContext = new RegistrationContext(container);

            bootstrap.Register(regContext);
        }

        private static Container SetupBootstrapContainer(BootstrapOptions options,
            IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var bootstrapContainer = new Container();

            bootstrapContainer.Register<IBootstrapRegister, MainRegister>();
            bootstrapContainer.RegisterInstance(options);
            bootstrapContainer.RegisterInstance(env);
            bootstrapContainer.RegisterInstance(loggerFactory);

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
    }
 }
