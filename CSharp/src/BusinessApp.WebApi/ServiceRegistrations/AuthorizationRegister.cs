namespace BusinessApp.WebApi
{
    using System;
    using System.Reflection;
    using BusinessApp.App;
    using SimpleInjector;

    /// <summary>
    /// Registers authorization based on the <see cref="AuthorizeAttribute" />
    /// </summary>
    public class AuthorizationRegister: IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public AuthorizationRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.RegisterConditional(
                typeof(IAuthorizer<>),
                typeof(AuthorizeAttributeHandler<>),
                IsAuthCommand);

            inner.Register(context);
        }

        private static bool IsAuthCommand(PredicateContext ctx)
        {
            var requestType = ctx.ServiceType.GetGenericArguments()[0];

            static bool HasAuthAttribute(Type targetType)
            {
                return targetType.GetCustomAttribute<AuthorizeAttribute>() != null;
            }

            while(!HasAuthAttribute(requestType) && requestType.IsConstructedGenericType)
            {
                requestType = requestType.GetGenericArguments()[0];
            }

            return HasAuthAttribute(requestType);
        }
    }
}
