using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EMRNext.Core.Domain.Entities.Portal;

namespace EMRNext.Infrastructure.Data.Configurations
{
    public class GroupAppointmentConfiguration : IEntityTypeConfiguration<GroupAppointment>
    {
        public void Configure(EntityTypeBuilder<GroupAppointment> builder)
        {
            builder.ToTable("GroupAppointments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Location)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Notes)
                .HasMaxLength(2000);

            builder.Property(e => e.CancellationReason)
                .HasMaxLength(500);

            builder.Property(e => e.MeetingLink)
                .HasMaxLength(500);

            builder.Property(e => e.SessionMaterials)
                .HasMaxLength(2000);

            // Indexes
            builder.HasIndex(e => e.GroupSeriesId);
            builder.HasIndex(e => e.StartTime);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => new { e.GroupSeriesId, e.StartTime });

            // Relationships
            builder.HasOne(e => e.GroupSeries)
                .WithMany(s => s.Sessions)
                .HasForeignKey(e => e.GroupSeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.AppointmentType)
                .WithMany()
                .HasForeignKey(e => e.AppointmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.BackupProvider)
                .WithMany()
                .HasForeignKey(e => e.BackupProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Query Filters
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
