using BusinessApp.CompositionRoot;
using BusinessApp.Infrastructure.Json;
using BusinessApp.WebApi.ProblemDetails;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi.Json
{
    /// <summary>
    /// Register web api specific JSON services
    /// </summary>
    public class JsonRegister : IBootstrapRegister
    {
        private static readonly ProblemDetailOptions deserializationProblemOptions =
            new(typeof(JsonDeserializationException), StatusCodes.Status422UnprocessableEntity)
            {
                MessageOverride = "Your data could not be read. The most likely causes is an invalid " +
                    "json structure or incorrect data type in a field (e.g Using a string when number is expected)"
            };

        private readonly IBootstrapRegister inner;

        public JsonRegister(IBootstrapRegister inner) => this.inner = inner;

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

#if DEBUG
            container.RegisterSingleton<IHttpRequestAnalyzer, JsonHttpRequestAnalyzer>();
#elif hasbatch
            container.RegisterSingleton<IHttpRequestAnalyzer, JsonHttpRequestAnalyzer>();
#endif

            ProblemDetailOptionBootstrap.AddProblem(deserializationProblemOptions);

            container.RegisterDecorator(typeof(IHttpRequestHandler<,>),
                typeof(JsonHttpDecorator<,>));

            inner.Register(context);
        }
    }
}
