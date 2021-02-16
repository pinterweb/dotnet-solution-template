namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using BusinessApp.WebApi.ProblemDetails;
    using System.Collections.Generic;

    public class ErrorRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public ErrorRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            inner.Register(context);

            container.RegisterInstance(ProblemDetailOptionBootstrap.KnownProblems);

            container.RegisterSingleton<IProblemDetailFactory, ProblemDetailFactory>();

            container.RegisterDecorator<IProblemDetailFactory, ProblemDetailFactoryHttpDecorator>(
                Lifestyle.Singleton);
        }
    }
}
