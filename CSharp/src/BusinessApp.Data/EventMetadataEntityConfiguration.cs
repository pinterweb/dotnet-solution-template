namespace BusinessApp.Data
{
    using BusinessApp.Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public abstract class EventMetadataEntityConfiguration<T> :
        IEntityTypeConfiguration<EventMetadata<T>>
        where T : class, IDomainEvent
    {
        protected abstract string TableName { get; }

        public void Configure(EntityTypeBuilder<EventMetadata<T>> builder)
        {
            builder.ToTable(TableName, "evt");

            builder.Property(p => p.Id)
                .HasColumnName("EventMetadataId")
                .HasConversion(id => (long)id, val => new MetadataId(val));

            builder.Property(p => p.CausationId)
                .HasColumnName("CausationId")
                .HasConversion(id => (long)id, val => new MetadataId(val));

            builder.Property(p => p.CorrelationId)
                .HasColumnName("CorrelationId")
                .HasConversion(id => (long)id, val => new MetadataId(val));

            builder.Property(p => p.EventName)
                .HasColumnType("varchar(500)")
                .IsRequired();

            builder.Property(i => i.OccurredUtc)
                .HasColumnType("datetimeoffset(0)")
                .IsRequired();

            var owned = builder.OwnsOne(o => o.Event);

            ConfigureEvent(owned);
        }

        protected virtual void ConfigureEvent(OwnedNavigationBuilder<EventMetadata<T>, T> builder)
        {

        }
    }
}
