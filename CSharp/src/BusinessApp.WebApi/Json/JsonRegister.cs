namespace BusinessApp.WebApi.Json
{
    using BusinessApp.App;
    using BusinessApp.App.Json;
    using BusinessApp.WebApi.ProblemDetails;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

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

            container.RegisterSingleton<ISerializer, NewtonsoftJsonSerializer>();

            ProblemDetailOptionBootstrap.KnownProblems.Add(JsonProblemDetailOption);

            container.RegisterInstance(
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),

                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
//#if DEBUG
                    Formatting = Formatting.Indented,
//#endif

                    Converters = new JsonConverter[]
                    {
                        new EntityIdJsonConverter(),
                        new LongToStringJsonConverter(),
                        new IDictionaryJsonConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                    }
                }
            );

            inner.Register(context);
        }
    }
}
