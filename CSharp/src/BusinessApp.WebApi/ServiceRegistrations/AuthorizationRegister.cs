namespace BusinessApp.WebApi
{
    using BusinessApp.App;

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

            container.Register(typeof(IAuthorizer<>), typeof(AuthorizeAttributeHandler<>));

            inner.Register(context);
        }
    }
}
