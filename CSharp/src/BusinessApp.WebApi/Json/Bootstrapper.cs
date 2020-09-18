namespace BusinessApp.WebApi.Json
{
    using BusinessApp.App;
    using BusinessApp.App.Json;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using SimpleInjector;

    /// <summary>
    /// Sets up the application to use json parsing
    /// </summary>
    public static class Bootstrapper
    {
        public static void Bootstrap(Container container)
        {
            container.RegisterDecorator(typeof(IResourceHandler<,>), typeof(JsonResponseDecorator<,>));
            container.RegisterSingleton<ISerializer, NewtonsoftJsonSerializer>();

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
                        new EntityIdJsonConverter<long>(),
                        new EntityIdJsonConverter<string>(),
                        new EntityIdJsonConverter<int>(),
                        new LongToStringJsonConverter(),
                        new IDictionaryJsonConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                    }
                }
            );
        }
    }
}
