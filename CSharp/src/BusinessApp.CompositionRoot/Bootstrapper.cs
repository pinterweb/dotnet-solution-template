using System.Linq;
using BusinessApp.Infrastructure;
using SimpleInjector;
using System.Collections.Generic;
using System;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Entry point to this assembly
    /// </summary>
    public static class Bootstrapper
    {
        public static void RegisterServices(Container container, RegistrationOptions options,
            IDictionary<Type, object> bootstrapServices)
        {
            var bootstrapContainer = SetupBootstrapContainer(options, bootstrapServices);

            RegisterBootstrapDecorators(bootstrapContainer, options);

            bootstrapContainer.Verify();

            var bootstrap = bootstrapContainer.GetInstance<IBootstrapRegister>();
            var regContext = new RegistrationContext(container);

            bootstrap.Register(regContext);

            // run this once on the outer most context
            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(RequestExceptionDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));
        }

        private static Container SetupBootstrapContainer(RegistrationOptions options,
            IDictionary<Type, object> bootstrapServices)
        {
            var bootstrapContainer = new Container();

            bootstrapContainer.Register<IBootstrapRegister, MainRegister>();
            bootstrapContainer.RegisterInstance(options);

            foreach (var svc in bootstrapServices)
            {
                bootstrapContainer.RegisterInstance(svc.Key, svc.Value);
            }

            return bootstrapContainer;
        }

        private static void RegisterBootstrapDecorators(Container container,
            RegistrationOptions options)
        {
            var bootstrapDecorators = container
                .GetTypesToRegister(typeof(IBootstrapRegister),
                    options.RegistrationAssemblies.Concat(new[] { typeof(Bootstrapper).Assembly }),
                    new TypesToRegisterOptions
                    {
                        IncludeDecorators = true
                    })
                .Where(type => type != typeof(MainRegister));

            foreach (var type in bootstrapDecorators)
            {
                container.RegisterDecorator(typeof(IBootstrapRegister), type);
            }
        }
    }
}
