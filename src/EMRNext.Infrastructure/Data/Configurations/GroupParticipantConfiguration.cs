using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EMRNext.Core.Domain.Entities.Portal;

namespace EMRNext.Infrastructure.Data.Configurations
{
    public class GroupParticipantConfiguration : IEntityTypeConfiguration<GroupParticipant>
    {
        public void Configure(EntityTypeBuilder<GroupParticipant> builder)
        {
            builder.ToTable("GroupParticipants");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Notes)
                .HasMaxLength(2000);

            builder.Property(e => e.EnrollmentStatus)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.CancellationReason)
                .HasMaxLength(500);

            builder.Property(e => e.ParticipationNotes)
                .HasMaxLength(2000);

            builder.Property(e => e.ProgressNotes)
                .HasMaxLength(2000);

            builder.Property(e => e.Goals)
                .HasMaxLength(2000);

            builder.Property(e => e.Interventions)
                .HasMaxLength(2000);

            // Indexes
            builder.HasIndex(e => e.GroupAppointmentId);
            builder.HasIndex(e => e.PatientId);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.IsWaitlisted);
            builder.HasIndex(e => new { e.GroupAppointmentId, e.PatientId }).IsUnique();

            // Relationships
            builder.HasOne(e => e.GroupAppointment)
                .WithMany(a => a.Participants)
                .HasForeignKey(e => e.GroupAppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query Filters
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
