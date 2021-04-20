using BusinessApp.Kernel;
using BusinessApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessApp.Infrastructure.EntityFramework
{
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
                .HasConversion(id => (long)id, val => new MetadataId(val))
                .IsRequired();

            builder.Property(p => p.CausationId)
                .HasColumnName("CausationId")
                .HasConversion(id => (long)id, val => new MetadataId(val))
                .IsRequired();

            builder.Property(p => p.CorrelationId)
                .HasColumnName("CorrelationId")
                .HasConversion(id => (long)id, val => new MetadataId(val))
                .IsRequired();

            builder.Property(p => p.EventName)
                .HasColumnType("varchar(500)")
                .IsRequired();

            builder.Property(i => i.OccurredUtc)
                .HasColumnType("datetimeoffset(0)")
                .IsRequired();

            builder.HasOne<Metadata>()
                .WithOne()
                .HasForeignKey<EventMetadata<T>>(e => e.CorrelationId);

            var owned = builder.OwnsOne(o => o.Event);

            ConfigureEvent(owned);
        }

        protected abstract void ConfigureEvent(OwnedNavigationBuilder<EventMetadata<T>, T> builder);
    }
}
