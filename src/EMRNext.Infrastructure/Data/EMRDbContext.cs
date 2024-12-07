using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Infrastructure.Data
{
    public class EMRDbContext : DbContext
    {
        public EMRDbContext(DbContextOptions<EMRDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Encounter> Encounters { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<Vital> Vitals { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<Procedure> Procedures { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderResult> OrderResults { get; set; }
        
        // Newly added entities
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Immunization> Immunizations { get; set; }
        public DbSet<FamilyHistory> FamilyHistories { get; set; }
        public DbSet<SocialHistory> SocialHistories { get; set; }
        public DbSet<Consent> Consents { get; set; }
        public DbSet<AdvanceDirective> AdvanceDirectives { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<LabOrder> LabOrders { get; set; }
        public DbSet<LabResult> LabResults { get; set; }
        public DbSet<CarePlan> CarePlans { get; set; }
        public DbSet<CarePlanActivity> CarePlanActivities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ClinicalNote> ClinicalNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Patient Configuration
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PublicId).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DateOfBirth).IsRequired();
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SocialSecurityNumber).HasMaxLength(50);
                
                entity.HasMany(e => e.Encounters)
                    .WithOne(e => e.Patient)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Encounter Configuration
            modelBuilder.Entity<Encounter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PublicId).IsUnique();
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.ClassCode).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.Provider)
                    .WithMany(p => p.Encounters)
                    .HasForeignKey(e => e.ProviderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Supervisor)
                    .WithMany(p => p.SupervisedEncounters)
                    .HasForeignKey(e => e.SupervisorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Provider Configuration
            modelBuilder.Entity<Provider>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NPI).HasMaxLength(50);
            });

            // Facility Configuration
            modelBuilder.Entity<Facility>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.NPI).HasMaxLength(50);
                entity.Property(e => e.TaxId).HasMaxLength(50);
            });

            // Diagnosis Configuration
            modelBuilder.Entity<Diagnosis>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DateDiagnosed).IsRequired();
                
                entity.HasOne(e => e.Encounter)
                    .WithMany(e => e.Diagnoses)
                    .HasForeignKey(e => e.EncounterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Procedure Configuration
            modelBuilder.Entity<Procedure>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ServiceDate).IsRequired();
                
                entity.HasOne(e => e.Encounter)
                    .WithMany(e => e.Procedures)
                    .HasForeignKey(e => e.EncounterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Prescription Configuration
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DrugName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.StartDate).IsRequired();
                
                entity.HasOne(e => e.Encounter)
                    .WithMany(e => e.Prescriptions)
                    .HasForeignKey(e => e.EncounterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OrderDate).IsRequired();
                
                entity.HasOne(e => e.Encounter)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(e => e.EncounterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.OrderDetails)
                    .WithOne(e => e.Order)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.OrderResults)
                    .WithOne(e => e.Order)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries();
            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditableEntity auditable)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditable.CreatedAt = now;
                            auditable.CreatedBy = GetCurrentUser();
                            break;
                        case EntityState.Modified:
                            auditable.ModifiedAt = now;
                            auditable.ModifiedBy = GetCurrentUser();
                            break;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        private string GetCurrentUser()
        {
            // TODO: Implement user context to get current user
            return "system";
        }
    }

    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        string CreatedBy { get; set; }
        DateTime? ModifiedAt { get; set; }
        string ModifiedBy { get; set; }
    }
}
