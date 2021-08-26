using Microsoft.Extensions.Configuration;
using BusinessApp.Test.Shared;

namespace BusinessApp.Infrastructure.Persistence.IntegrationTest
{
    public class DbDatabaseFixture : DatabaseFixture
    {
        protected override string ConnectionStringName { get; } = "DbTest";

        protected override void Configure(IConfigurationBuilder builder) =>
            builder.AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables(prefix: "BusinessApp_");
    }
}
