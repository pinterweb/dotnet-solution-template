namespace BusinessApp.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventMetadataEntityConfiguration : IEntityTypeConfiguration<EventMetadata>
    {
        public void Configure(EntityTypeBuilder<EventMetadata> builder)
        {
            builder.ToTable("EventMetadata", "evt");

            builder.Property(p => p.Id)
                .HasColumnName("EventId")
                .HasConversion(id => id.ToInt64(null), val => new EventId(val));

            builder.Property(p => p.CorrelationId)
                .HasConversion(id => (long)id, val => new EventId(val));

            builder.Property(p => p.EventCreator)
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(p => p.EventDisplayText)
                .HasColumnType("varchar(500)")
                .IsRequired();

            builder.Property(i => i.OccurredUtc)
                .HasColumnType("datetimeoffset(0)")
                .IsRequired();
        }
    }
}
