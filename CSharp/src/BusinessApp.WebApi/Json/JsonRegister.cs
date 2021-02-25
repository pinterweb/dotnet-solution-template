namespace BusinessApp.WebApi.Json
{
    using BusinessApp.WebApi.ProblemDetails;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    public class JsonRegister : IBootstrapRegister
    {
        private static ProblemDetailOptions JsonProblemDetailOption =
            new ProblemDetailOptions
            {
                ProblemType = typeof(JsonException),
                StatusCode = StatusCodes.Status400BadRequest,
                MessageOverride = "Data is not in the correct format"
            };
        private readonly IBootstrapRegister inner;

        public JsonRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.RegisterDecorator(typeof(IHttpRequestHandler<,>),
                typeof(JsonHttpDecorator<,>));

            ProblemDetailOptionBootstrap.KnownProblems.Add(JsonProblemDetailOption);

            inner.Register(context);
        }
    }
}
