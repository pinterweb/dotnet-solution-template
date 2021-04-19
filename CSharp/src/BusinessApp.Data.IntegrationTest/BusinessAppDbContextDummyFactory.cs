using FakeItEasy;
using Microsoft.EntityFrameworkCore;

namespace BusinessApp.Data.IntegrationTest
{
    public class BusinessAppDbContextDummyFactory : DummyFactory<BusinessAppDbContext>
    {
        public static BusinessAppDbContext Real()
        {
            return new BusinessAppDbContext(
                new DbContextOptionsBuilder<BusinessAppDbContext>()
                    .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
                    .Options
            );
        }

        protected override BusinessAppDbContext Create() => Real();
    }
}
