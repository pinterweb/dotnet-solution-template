namespace BusinessApp.WebApi.Json
{
    using BusinessApp.App;
    using BusinessApp.App.Json;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using SimpleInjector;

    public class NewtonsoftJsonRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public NewtonsoftJsonRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.RegisterConditional<ISerializer, NewtonsoftJsonSerializer>(
                Lifestyle.Singleton,
                c => !c.Handled);

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
                        new NewtonsoftEntityIdJsonConverter(),
                        new NewtonsoftLongToStringJsonConverter(),
                        new NewtonsoftIDictionaryJsonConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                    }
                }
            );

            inner.Register(context);
        }
    }
}