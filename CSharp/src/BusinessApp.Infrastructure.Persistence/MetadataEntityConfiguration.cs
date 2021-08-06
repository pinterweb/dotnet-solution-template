using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable IDE0058
namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Entity configuration to save metadata
    /// </summary>
    public class MetadataEntityConfiguration : IEntityTypeConfiguration<Metadata>
    {
        public void Configure(EntityTypeBuilder<Metadata> builder)
        {
            builder.ToTable("Metadata", "dbo");

            builder.Property(i => i.Id)
                .HasColumnName("MetadataId")
                .HasConversion(id => (long)id, val => new MetadataId(val));

            builder.Property(p => p.Username)
                .HasColumnName("Username")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(p => p.DataSetName)
                .HasColumnName("DataSetName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(p => p.TypeName)
                .HasColumnName("TypeName")
                .HasColumnType("varchar(100)")
                .IsRequired();

            builder.Property(p => p.OccurredUtc)
                .HasColumnName("OccurredUtc")
                .HasColumnType("datetimeoffset(0)");

            builder.HasDiscriminator(m => m.DataSetName);
        }
    }

    public abstract class MetadataEntityConfiguration<T> : IEntityTypeConfiguration<T>
        where T : class
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
            => builder.HasOne<Metadata<T>>()
                .WithOne(m => m.Data)
                .HasForeignKey<T>("MetadataId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
    }
}
#pragma warning restore IDE0058
