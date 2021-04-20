using System.Text.Json;
using System.Text.Json.Serialization;
using BusinessApp.Infrastructure;
using BusinessApp.Infrastructure.Json;
using SimpleInjector;

namespace BusinessApp.CompositionRoot
{
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
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
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
