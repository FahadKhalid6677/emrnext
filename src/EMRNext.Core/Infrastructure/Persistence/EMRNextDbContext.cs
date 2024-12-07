using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using EMRNext.Core.Identity;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;

namespace EMRNext.Core.Infrastructure.Persistence
{
    /// <summary>
    /// Primary database context for EMRNext
    /// </summary>
    public class EMRNextDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public EMRNextDbContext(DbContextOptions<EMRNextDbContext> options)
            : base(options)
        {
        }

        // Domain Entities
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Encounter> Encounters { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }

        // Clinical Models
        public DbSet<ClinicalDocumentation> ClinicalDocumentations { get; set; }
        public DbSet<VitalSign> VitalSigns { get; set; }

        /// <summary>
        /// Configure entity relationships and constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Patient Configuration
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                
                entity.HasMany(p => p.Encounters)
                      .WithOne()
                      .HasForeignKey("PatientId")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Encounter Configuration
            modelBuilder.Entity<Encounter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            // Prescription Configuration
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
            });

            // Apply global query filters
            modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Encounter>().HasQueryFilter(e => !e.IsDeleted);
        }

        /// <summary>
        /// Add soft delete and audit trail support
        /// </summary>
        public override int SaveChanges()
        {
            ApplySoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplySoftDelete()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    
                    if (entry.Entity is Entity<Guid> entity)
                    {
                        entity.IsDeleted = true;
                        entity.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Application Role for Identity
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
