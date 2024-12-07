using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Core.Domain.Entities.Identity;
using EMRNext.Core.Domain.Entities.Clinical;
using EMRNext.Infrastructure.Data.Configurations;

namespace EMRNext.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Identity
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        // Portal
        public DbSet<GroupAppointment> GroupAppointments { get; set; }
        public DbSet<GroupParticipant> GroupParticipants { get; set; }
        public DbSet<GroupSeries> GroupSeries { get; set; }
        public DbSet<ParticipantReport> ParticipantReports { get; set; }
        public DbSet<SeriesOutcome> SeriesOutcomes { get; set; }

        // Clinical
        public DbSet<LabOrderEntity> LabOrders { get; set; }
        public DbSet<LabTestOrderEntity> LabTestOrders { get; set; }
        public DbSet<LabTestDefinitionEntity> LabTestDefinitions { get; set; }
        public DbSet<LabReferenceRangeEntity> LabReferenceRanges { get; set; }
        public DbSet<LabResultEntity> LabResults { get; set; }
        public DbSet<ImagingOrderEntity> ImagingOrders { get; set; }
        public DbSet<ImagingResultEntity> ImagingResults { get; set; }
        public DbSet<ImagingStudyEntity> ImagingStudies { get; set; }
        public DbSet<ImagingSeriesEntity> ImagingSeries { get; set; }
        public DbSet<ImagingInstanceEntity> ImagingInstances { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<WaitlistEntry> WaitlistEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Identity configurations
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles")
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Portal configurations
            modelBuilder.ApplyConfiguration(new GroupAppointmentConfiguration());
            modelBuilder.ApplyConfiguration(new GroupParticipantConfiguration());

            // Clinical configurations
            modelBuilder.Entity<LabOrderEntity>()
                .HasMany(lo => lo.Tests)
                .WithOne(t => t.LabOrder)
                .HasForeignKey(t => t.LabOrderId);

            modelBuilder.Entity<LabTestOrderEntity>()
                .HasMany(t => t.Results)
                .WithOne(r => r.TestOrder)
                .HasForeignKey(r => r.LabTestOrderId);

            modelBuilder.Entity<LabTestDefinitionEntity>()
                .HasMany(t => t.ReferenceRanges)
                .WithOne(r => r.Test)
                .HasForeignKey(r => r.TestId);

            modelBuilder.Entity<ImagingOrderEntity>()
                .HasMany(o => o.Results)
                .WithOne(r => r.Order)
                .HasForeignKey(r => r.ImagingOrderId);

            modelBuilder.Entity<ImagingResultEntity>()
                .HasMany(r => r.Studies)
                .WithOne(s => s.Result)
                .HasForeignKey(s => s.ImagingResultId);

            modelBuilder.Entity<ImagingStudyEntity>()
                .HasMany(s => s.Series)
                .WithOne(s => s.Study)
                .HasForeignKey(s => s.ImagingStudyId);

            modelBuilder.Entity<ImagingSeriesEntity>()
                .HasMany(s => s.Instances)
                .WithOne(i => i.Series)
                .HasForeignKey(i => i.ImagingSeriesId);

            // Query filters
            modelBuilder.Entity<GroupAppointment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<GroupParticipant>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<GroupSeries>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(e => e.IsActive);
            modelBuilder.Entity<LabOrderEntity>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ImagingOrderEntity>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<TimeSlot>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<WaitlistEntry>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override int SaveChanges()
        {
            UpdateAuditableEntities();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditableEntities()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is AuditableEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (AuditableEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedDate = DateTime.UtcNow;
                    // Set CreatedBy from current user context
                }
                else
                {
                    entity.LastModifiedDate = DateTime.UtcNow;
                    // Set LastModifiedBy from current user context
                }
            }
        }
    }
}
