using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Infrastructure.Data
{
    public class EMRNextDbContext : DbContext
    {
        public EMRNextDbContext(DbContextOptions<EMRNextDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Encounter> Encounters { get; set; }
        public DbSet<ClinicalNote> ClinicalNotes { get; set; }
        public DbSet<GroupSeries> GroupSeries { get; set; }
        public DbSet<GroupAppointment> GroupAppointments { get; set; }
        public DbSet<GroupParticipant> GroupParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<ClinicalNote>()
                .HasOne<Patient>()
                .WithMany()
                .HasForeignKey(n => n.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClinicalNote>()
                .HasOne<Provider>()
                .WithMany()
                .HasForeignKey(n => n.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClinicalNote>()
                .HasOne<Encounter>()
                .WithMany()
                .HasForeignKey(n => n.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupAppointment>()
                .HasOne(a => a.GroupSeries)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.GroupSeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupParticipant>()
                .HasOne(p => p.GroupAppointment)
                .WithMany(a => a.Participants)
                .HasForeignKey(p => p.GroupAppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupParticipant>()
                .HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
