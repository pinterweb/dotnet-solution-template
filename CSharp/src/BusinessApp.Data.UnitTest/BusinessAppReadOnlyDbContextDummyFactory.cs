namespace BusinessApp.Data.UnitTest
{
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;

    public class BusinessAppReadOnlyDbContextDummyFactory : DummyFactory<BusinessAppReadOnlyDbContext>
    {
        protected override BusinessAppReadOnlyDbContext Create()
        {
            return new BusinessAppReadOnlyDbContext(new DbContextOptionsBuilder<BusinessAppReadOnlyDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
                .Options);
        }
    }
}
