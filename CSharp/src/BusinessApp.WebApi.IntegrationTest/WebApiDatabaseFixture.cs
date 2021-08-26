using Microsoft.Extensions.Configuration;
using BusinessApp.Test.Shared;

namespace BusinessApp.WebApi.IntegrationTest
{
    public class WebApiDatabaseFixture : DatabaseFixture
    {
        protected override string ConnectionStringName { get; } = "Main";

        protected override void Configure(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.test.json")
                .AddEnvironmentVariables(prefix: "BusinessApp_Test_");
        }
    }
}
