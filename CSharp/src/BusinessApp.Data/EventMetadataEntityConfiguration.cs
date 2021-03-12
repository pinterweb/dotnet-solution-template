namespace BusinessApp.Data
{
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventMetadataEntityConfiguration : IEntityTypeConfiguration<EventMetadata>
    {
        public void Configure(EntityTypeBuilder<EventMetadata> builder)
        {
            builder.ToTable("EventMetadata", "evt");

            builder.OwnsOne(e => e.Id, ob =>
            {
                ob.Property(p => p.Id)
                    .HasConversion(id => id.ToInt64(null), val => new Domain.EventId(val));

                ob.Property(p => p.CausationId)
                    .HasColumnName("CausationId")
                    .HasConversion(id => id.ToInt64(null), val => new Domain.EventId(val));

                ob.Property(p => p.CorrelationId)
                    .HasColumnName("CorrelationId")
                    .HasConversion(id => id.ToInt64(null), val => new Domain.EventId(val));
            });

            builder.Property(p => p.EventName)
                .HasColumnType("varchar(500)")
                .IsRequired();

            builder.Property(i => i.OccurredUtc)
                .HasColumnType("datetimeoffset(0)")
                .IsRequired();
        }
    }
}
