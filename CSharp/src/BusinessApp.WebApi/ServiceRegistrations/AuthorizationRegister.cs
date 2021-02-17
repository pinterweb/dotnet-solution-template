namespace BusinessApp.WebApi
{
    using System;
    using System.Reflection;
    using BusinessApp.App;
    using SimpleInjector;

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

            container.RegisterConditional(
                typeof(IAuthorizer<>),
                typeof(NullAuthorizer<>),
                c => !c.Handled);

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

        /// <summary>
        /// Null object pattern. When no authorization is used on a request
        /// </summary>
        private sealed class NullAuthorizer<T> : IAuthorizer<T>
        {
            public void AuthorizeObject(T instance)
            {}
        }
    }
}
