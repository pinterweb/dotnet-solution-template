using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace BusinessApp.Infrastructure.WebApi
{
    public class ExpandJsonConfigurationSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new ExpandJsonConfigurationProvider(this);
        }
    }

    public class ExpandJsonConfigurationProvider : JsonConfigurationProvider
    {
        public ExpandJsonConfigurationProvider(ExpandJsonConfigurationSource source)
            : base(source) { }

        public override void Load()
        {
            base.Load();
            Data = Data.ToDictionary(
                x => x.Key,
                x => Environment.ExpandEnvironmentVariables(x.Value));
        }
    }
}
