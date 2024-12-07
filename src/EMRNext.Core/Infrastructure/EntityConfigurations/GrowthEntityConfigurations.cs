using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Infrastructure.EntityConfigurations
{
    public class GrowthStandardEntityConfiguration : IEntityTypeConfiguration<GrowthStandardEntity>
    {
        public void Configure(EntityTypeBuilder<GrowthStandardEntity> builder)
        {
            builder.ToTable("GrowthStandards");
            
            builder.HasIndex(x => new { x.Type, x.Gender, x.Version })
                  .IsUnique();
            
            builder.Property(x => x.Name)
                  .IsRequired()
                  .HasMaxLength(100);
            
            builder.Property(x => x.Gender)
                  .IsRequired()
                  .HasMaxLength(1);

            builder.Property(x => x.MetadataJson)
                  .HasColumnType("jsonb");
        }
    }

    public class PercentileDataEntityConfiguration : IEntityTypeConfiguration<PercentileDataEntity>
    {
        public void Configure(EntityTypeBuilder<PercentileDataEntity> builder)
        {
            builder.ToTable("PercentileData");
            
            builder.HasIndex(x => new { x.GrowthStandardId, x.MeasurementType, x.Age });
            
            builder.Property(x => x.PercentileValuesJson)
                  .HasColumnType("jsonb");

            builder.HasOne(x => x.GrowthStandard)
                  .WithMany(x => x.PercentileData)
                  .HasForeignKey(x => x.GrowthStandardId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PatientMeasurementEntityConfiguration : IEntityTypeConfiguration<PatientMeasurementEntity>
    {
        public void Configure(EntityTypeBuilder<PatientMeasurementEntity> builder)
        {
            builder.ToTable("PatientMeasurements");
            
            builder.HasIndex(x => new { x.PatientId, x.Type, x.MeasurementDate });
            
            builder.Property(x => x.Value)
                  .HasPrecision(10, 4);

            builder.Property(x => x.MetadataJson)
                  .HasColumnType("jsonb");
        }
    }

    public class GrowthAlertEntityConfiguration : IEntityTypeConfiguration<GrowthAlertEntity>
    {
        public void Configure(EntityTypeBuilder<GrowthAlertEntity> builder)
        {
            builder.ToTable("GrowthAlerts");
            
            builder.HasIndex(x => new { x.PatientId, x.Type, x.DetectedDate });
            
            builder.Property(x => x.AlertType)
                  .IsRequired()
                  .HasMaxLength(50);

            builder.Property(x => x.Description)
                  .IsRequired()
                  .HasMaxLength(500);
        }
    }
}
