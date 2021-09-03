using BusinessApp.CompositionRoot;
using BusinessApp.Infrastructure.Json;
using BusinessApp.WebApi.ProblemDetails;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi.Json
{
    /// <summary>
    /// Register web api specific JSON services
    /// </summary>
    public class NewtonsoftRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public NewtonsoftRegister(IBootstrapRegister inner) => this.inner = inner;

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.RegisterDecorator<IProblemDetailFactory, NewtonsoftProblemDetailFactory>();

            inner.Register(context);
        }
    }
}
