namespace BusinessApp.Data.IntegrationTest
{
    using System;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class DummyEventModel : IDomainEvent
    {
        public int Id { get; set; }
        public DateTimeOffset Occurred => A.Dummy<DateTimeOffset>();

        public class DummyEventModelConfiguration : IEntityTypeConfiguration<DummyEventModel>
        {
            public void Configure(EntityTypeBuilder<DummyEventModel> builder)
            {}
        }
    }
}

