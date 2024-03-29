using System.Text.Json;
using System.Text.Json.Serialization;
using BusinessApp.Infrastructure;
using BusinessApp.Infrastructure.Json;
using SimpleInjector;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Registers services related to System.Test.Json
    /// </summary>
    public class SystemJsonRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public SystemJsonRegister(IBootstrapRegister inner) => this.inner = inner;

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
                        new SystemJsonEntityIdConverterFactory(),
                        new SystemJsonLongToStringConverter(),
                        new JsonStringEnumConverter()
                    }
                }
            );

            inner.Register(context);
        }
    }
}
