namespace BusinessApp.Data.IntegrationTest
{
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;

    public class BusinessAppReadOnlyDbContextDummyFactory : DummyFactory<BusinessAppReadOnlyDbContext>
    {
        protected override BusinessAppReadOnlyDbContext Create()
        {
            return new BusinessAppReadOnlyDbContext(new DbContextOptionsBuilder<BusinessAppReadOnlyDbContext>()
                // we already have UseSqlServer for our DatabaseFixutre
                // could use inmemory too, but would need nuget package
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
                .Options);
        }
    }
}
