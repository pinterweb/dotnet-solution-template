using Xunit;

namespace BusinessApp.WebApi.IntegrationTest
{
    [CollectionDefinition(nameof(DatabaseCollectionFixture))]
    public class DatabaseCollectionFixture : ICollectionFixture<WebApiDatabaseFixture>
    { }
}
