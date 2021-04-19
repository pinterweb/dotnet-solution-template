using Xunit;

namespace BusinessApp.App.IntegrationTest
{
    public class CommonFixture
    {
        public const string Name = "Common Fixture";
    }

    [CollectionDefinition(CommonFixture.Name)]
    public class CommonCollection : ICollectionFixture<CommonFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
