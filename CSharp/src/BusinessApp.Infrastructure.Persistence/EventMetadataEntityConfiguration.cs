using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable IDE0058
namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Entity configuration to save event metadata
    /// </summary>
    public abstract class EventMetadataEntityConfiguration<T> :
        IEntityTypeConfiguration<EventMetadata<T>>
        where T : class, IEvent
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

            builder.HasOne<Metadata<T>>()
                .WithMany()
                .HasForeignKey(e => e.CorrelationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            var owned = builder.OwnsOne(o => o.Event);

            owned.Ignore(o => o.OccurredUtc);

            ConfigureEvent(owned);
        }

        protected abstract void ConfigureEvent(OwnedNavigationBuilder<EventMetadata<T>, T> builder);
    }
}
#pragma warning restore IDE0058
