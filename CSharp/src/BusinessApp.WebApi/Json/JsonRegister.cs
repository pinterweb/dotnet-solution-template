using BusinessApp.CompositionRoot;
using BusinessApp.WebApi.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BusinessApp.WebApi.Json
{
    /// <summary>
    /// Register web api specific JSON services
    /// </summary>
    public class JsonRegister : IBootstrapRegister
    {
        private static readonly ProblemDetailOptions jsonProblemDetailOption =
            new(typeof(JsonException), StatusCodes.Status400BadRequest)
            {
                MessageOverride = "Data is not in the correct format"
            };
        private readonly IBootstrapRegister inner;

        public JsonRegister(IBootstrapRegister inner) => this.inner = inner;

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            ProblemDetailOptionBootstrap.AddProblem(jsonProblemDetailOption);

#if DEBUG
            container.RegisterSingleton<IHttpRequestAnalyzer, JsonHttpRequestAnalyzer>();
#elif hasbatch
            container.RegisterSingleton<IHttpRequestAnalyzer, JsonHttpRequestAnalyzer>();
#endif

            container.RegisterDecorator(typeof(IHttpRequestHandler<,>),
                typeof(JsonHttpDecorator<,>));

            inner.Register(context);
        }
    }
}
