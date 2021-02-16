namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using BusinessApp.App;
    using System.Linq;
    using System.Collections.Generic;
    using System;
    using System.Reflection;

    public partial class RequestRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;
        private readonly BootstrapOptions options;

        public RequestRegister(IBootstrapRegister inner, BootstrapOptions options)
        {
            this.inner = inner;
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            var handlers = RegisterConcreteHandlers(container);

            inner.Register(context);

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroScopeWrappingHandler<,>),
                ctx => ctx.Consumer?.ImplementationType.GetGenericTypeDefinition() == typeof(MacroBatchRequestDelegator<,,>));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(BatchRequestDelegator<,>),
                ctx => ctx.Consumer?.ImplementationType.GetGenericTypeDefinition() != typeof(MacroBatchRequestDelegator<,,>));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchRequestDelegator<,,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                c => CreateQueryType(c, container),
                Lifestyle.Singleton,
                c => !c.Handled && typeof(IQuery).IsAssignableFrom(c.ServiceType.GetGenericArguments()[0]));

            container.RegisterConditional(typeof(IRequestHandler<,>),
                c => CreateRequestHandler(c, container),
                Lifestyle.Scoped,
                c => !c.Handled);
        }

        private static Type CreateRequestHandler(TypeFactoryContext c, Container container)
        {
            var requestType = c.ServiceType.GetGenericArguments()[0];
            var responseType = c.ServiceType.GetGenericArguments()[1];

            var concreteType = container.GetRootRegistrations()
                .FirstOrDefault(reg => reg.ServiceType.GetInterfaces().Any(i => i == c.ServiceType))?
                .ServiceType;

            if (concreteType == null)
            {
                throw new BusinessAppWebApiException(
                    $"No command handler is setup for command '{requestType.Name}'. Please set one up.");
            }
            else
            {
                return c.Consumer?.ImplementationType.GetGenericTypeDefinition() == typeof(BatchRequestDelegator<,>)
                    ? typeof(BatchScopeWrappingHandler<,,>).MakeGenericType(concreteType, requestType, responseType)
                    : concreteType;
            }
        }

        private static Type CreateQueryType(TypeFactoryContext c, Container container)
        {
            var requestType = c.ServiceType.GetGenericArguments()[0];
            var responseType = c.ServiceType.GetGenericArguments()[1];
            var concreteType = container.GetRootRegistrations()
                .FirstOrDefault(reg => reg.ServiceType.GetInterfaces().Any(i => i == c.ServiceType))?
                .ServiceType;
            var fallbackType = container.GetRegistration(
                typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(IEnumerable<>).MakeGenericType(responseType)));

            if (concreteType != null) return concreteType;

            if (fallbackType == null)
            {
                throw new BusinessAppWebApiException(
                    $"No query handler is setup for '{requestType.Name}'. Please set one up.");
            }

            return typeof(SingleQueryDelegator<,,>).MakeGenericType(
                fallbackType.Registration.ImplementationType,
                requestType,
                responseType);
        }

        private IEnumerable<Type> RegisterConcreteHandlers(Container container)
        {
            var handlers = container.GetTypesToRegister(typeof(IRequestHandler<,>),
                options.RegistrationAssemblies,
                new TypesToRegisterOptions
                {
                    IncludeGenericTypeDefinitions = true,
                    IncludeComposites = false,
                });

            foreach (var handlerType in handlers)
            {
                container.Register(handlerType);
            }

            return handlers;
        }
    }
}
