using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Entity configuration to save metadata
    /// </summary>
    public class MetadataEntityConfiguration : IEntityTypeConfiguration<Metadata>
    {
        public void Configure(EntityTypeBuilder<Metadata> builder)
        {
            _ = builder.ToTable("Metadata", "dbo");

            _ = builder.Property(i => i.Id)
                .HasColumnName("MetadataId")
                .HasConversion(id => (long)id, val => new MetadataId(val));

            _ = builder.Property(p => p.Username)
                .HasColumnName("Username")
                .HasColumnType("varchar(100)")
                .IsRequired();

            _ = builder.Property(p => p.DataSetName)
                .HasColumnName("DataSetName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            _ = builder.Property(p => p.TypeName)
                .HasColumnName("TypeName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            _ = builder.Property(p => p.OccurredUtc)
                .HasColumnName("OccurredUtc")
                .HasColumnType("datetimeoffset(0)");
        }
    }

    /// <summary>
    /// Entity configuration to setup a discriminator on the metadata table
    /// and relate the two tables correctly
    /// </summary>
    /// <remarks>Use if metadata is saved to a different table</remarks>
    public abstract class MetadataEntityConfiguration<T> :
        IEntityTypeConfiguration<Metadata<T>>,
        IEntityTypeConfiguration<T>
        where T : class
    {
        public virtual string MetadataDiscriminatorValue { get; } =
            $"{typeof(T).Namespace}.{typeof(T).Name}";

        public void Configure(EntityTypeBuilder<Metadata<T>> builder)
        {
            _ = builder.HasDiscriminator(m => m.DataSetName)
                .HasValue(MetadataDiscriminatorValue);
        }

        public void Configure(EntityTypeBuilder<T> builder)
        {
            _ = builder.HasOne<Metadata<T>>()
                .WithOne(m => m.Data)
                .HasForeignKey<T>("MetadataId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            ConfigureData(builder);
        }

        protected abstract void ConfigureData(EntityTypeBuilder<T> builder);
    }
}
