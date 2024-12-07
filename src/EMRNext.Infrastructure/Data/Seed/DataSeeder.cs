using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Entities.Portal;

namespace EMRNext.Infrastructure.Data.Seed
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public DataSeeder(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync()
        {
            // Only seed if the database is empty
            if (!_context.Users.Any())
            {
                await SeedUsersAsync();
                await SeedRolesAsync();
                await SeedUserRolesAsync();
                await SeedInitialDataAsync();
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedUsersAsync()
        {
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "admin@emrnext.com",
                    FirstName = "Admin",
                    LastName = "User",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "doctor@emrnext.com",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var user in users)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, "Admin123!");
                await _context.Users.AddAsync(user);
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid().ToString(), Name = "Admin" },
                new Role { Id = Guid.NewGuid().ToString(), Name = "Doctor" },
                new Role { Id = Guid.NewGuid().ToString(), Name = "Nurse" },
                new Role { Id = Guid.NewGuid().ToString(), Name = "Patient" }
            };

            await _context.Roles.AddRangeAsync(roles);
        }

        private async Task SeedUserRolesAsync()
        {
            var adminUser = await _context.Users.FirstAsync(u => u.Email == "admin@emrnext.com");
            var doctorUser = await _context.Users.FirstAsync(u => u.Email == "doctor@emrnext.com");
            var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
            var doctorRole = await _context.Roles.FirstAsync(r => r.Name == "Doctor");

            await _context.UserRoles.AddRangeAsync(new List<UserRole>
            {
                new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id },
                new UserRole { UserId = doctorUser.Id, RoleId = doctorRole.Id }
            });
        }

        private async Task SeedInitialDataAsync()
        {
            // Seed sample patients
            var patients = new List<Patient>
            {
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Alice",
                    LastName = "Johnson",
                    DateOfBirth = DateTime.Parse("1985-03-15"),
                    Gender = "Female",
                    Email = "alice.j@example.com",
                    PhoneNumber = "555-0101",
                    Address = "123 Main St",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Patient
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Bob",
                    LastName = "Smith",
                    DateOfBirth = DateTime.Parse("1990-07-22"),
                    Gender = "Male",
                    Email = "bob.s@example.com",
                    PhoneNumber = "555-0102",
                    Address = "456 Oak Ave",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                }
            };

            await _context.Patients.AddRangeAsync(patients);

            // Seed sample appointments
            var doctor = await _context.Users.FirstAsync(u => u.Email == "doctor@emrnext.com");
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Id = Guid.NewGuid(),
                    PatientId = patients[0].Id,
                    DoctorId = doctor.Id,
                    StartTime = DateTime.UtcNow.AddDays(1).Date.AddHours(9),
                    EndTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
                    Status = "Scheduled",
                    Type = "Initial Consultation",
                    Notes = "First visit - general checkup",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Appointment
                {
                    Id = Guid.NewGuid(),
                    PatientId = patients[1].Id,
                    DoctorId = doctor.Id,
                    StartTime = DateTime.UtcNow.AddDays(2).Date.AddHours(14),
                    EndTime = DateTime.UtcNow.AddDays(2).Date.AddHours(15),
                    Status = "Scheduled",
                    Type = "Follow-up",
                    Notes = "Follow-up for previous treatment",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                }
            };

            await _context.Appointments.AddRangeAsync(appointments);

            // Seed sample tasks
            var tasks = new List<Task>
            {
                new Task
                {
                    Id = Guid.NewGuid(),
                    Title = "Review Lab Results",
                    Description = "Review and document lab results for Alice Johnson",
                    DueDate = DateTime.UtcNow.AddDays(1),
                    Priority = "High",
                    Status = "Pending",
                    AssignedToId = doctor.Id,
                    PatientId = patients[0].Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                },
                new Task
                {
                    Id = Guid.NewGuid(),
                    Title = "Update Treatment Plan",
                    Description = "Update treatment plan for Bob Smith based on latest consultation",
                    DueDate = DateTime.UtcNow.AddDays(3),
                    Priority = "Medium",
                    Status = "Pending",
                    AssignedToId = doctor.Id,
                    PatientId = patients[1].Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                }
            };

            await _context.Tasks.AddRangeAsync(tasks);
        }
    }
}
