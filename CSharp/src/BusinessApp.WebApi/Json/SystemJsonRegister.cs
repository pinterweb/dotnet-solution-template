namespace BusinessApp.WebApi.Json
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using BusinessApp.App;
    using BusinessApp.App.Json;
    using SimpleInjector;

    public class SystemJsonRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public SystemJsonRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.RegisterConditional<ISerializer, SystemJsonSerializerAdapter>(
                Lifestyle.Singleton,
                c => !c.Handled);

            container.RegisterInstance(
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//#if DEBUG
                    WriteIndented = true,
//#endif
                    Converters =
                    {
                        new SystemEntityIdJsonConverterFactory(),
                        new SystemLongToStringJsonConverter(),
                        new JsonStringEnumConverter()
                    }
                }
            );

            inner.Register(context);
        }
    }
}
