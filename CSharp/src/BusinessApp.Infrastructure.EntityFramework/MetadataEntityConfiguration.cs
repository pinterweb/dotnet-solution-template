using BusinessApp.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable IDE0058
namespace BusinessApp.Infrastructure.EntityFramework
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
        }
    }

    public abstract class MetadataEntityConfiguration<T> : IEntityTypeConfiguration<T>
        where T : class
    {
        protected abstract string TableName { get; }

        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.ToTable(TableName, "dbo");

            builder.HasOne<Metadata<T>>()
                .WithOne(e => e.Data)
                .HasForeignKey<T>("MetadataId");
        }
    }
}
#pragma warning restore IDE0058
