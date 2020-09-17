namespace BusinessApp.Data.IntegrationTest
{
    using BusinessApp.Domain;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;

    public class BusinessAppDbContextDummyFactory : DummyFactory<BusinessAppDbContext>
    {
        public static BusinessAppDbContext Real()
        {
            return new BusinessAppDbContext(
                new DbContextOptionsBuilder<BusinessAppDbContext>()
                    .UseSqlServer("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
                    .Options,
                A.Dummy<EventUnitOfWork>());
        }

        protected override BusinessAppDbContext Create() => Real();
    }
}
