using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities.Identity;
using EMRNext.Core.Domain.Entities.Clinical;
using EMRNext.Core.Domain.Entities.Portal;
using Microsoft.AspNetCore.Identity;

namespace EMRNext.Infrastructure.Data
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
                await SeedRolesAsync();
                await SeedUsersAsync();
                await SeedLabTestDefinitionsAsync();
                await SeedTimeSlots();
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new List<Role>
            {
                new Role { Name = "Admin", NormalizedName = "ADMIN" },
                new Role { Name = "Doctor", NormalizedName = "DOCTOR" },
                new Role { Name = "Nurse", NormalizedName = "NURSE" },
                new Role { Name = "Patient", NormalizedName = "PATIENT" },
                new Role { Name = "Receptionist", NormalizedName = "RECEPTIONIST" }
            };

            await _context.Roles.AddRangeAsync(roles);
        }

        private async Task SeedUsersAsync()
        {
            var adminUser = new User
            {
                UserName = "admin@emrnext.com",
                Email = "admin@emrnext.com",
                NormalizedUserName = "ADMIN@EMRNEXT.COM",
                NormalizedEmail = "ADMIN@EMRNEXT.COM",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true
            };

            adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "Admin123!");
            await _context.Users.AddAsync(adminUser);

            var doctorUser = new User
            {
                UserName = "doctor@emrnext.com",
                Email = "doctor@emrnext.com",
                NormalizedUserName = "DOCTOR@EMRNEXT.COM",
                NormalizedEmail = "DOCTOR@EMRNEXT.COM",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Smith",
                IsActive = true
            };

            doctorUser.PasswordHash = _passwordHasher.HashPassword(doctorUser, "Doctor123!");
            await _context.Users.AddAsync(doctorUser);

            // Add user roles
            await _context.UserRoles.AddRangeAsync(new List<UserRole>
            {
                new UserRole { UserId = adminUser.Id, RoleId = _context.Roles.First(r => r.Name == "Admin").Id },
                new UserRole { UserId = doctorUser.Id, RoleId = _context.Roles.First(r => r.Name == "Doctor").Id }
            });
        }

        private async Task SeedLabTestDefinitionsAsync()
        {
            var labTests = new List<LabTestDefinitionEntity>
            {
                new LabTestDefinitionEntity
                {
                    Name = "Complete Blood Count",
                    Code = "CBC",
                    Description = "Measures various components and features of blood",
                    Category = "Hematology",
                    ReferenceRanges = new List<LabReferenceRangeEntity>
                    {
                        new LabReferenceRangeEntity
                        {
                            Component = "WBC",
                            LowValue = 4.5,
                            HighValue = 11.0,
                            Unit = "10^9/L",
                            Gender = "All"
                        },
                        new LabReferenceRangeEntity
                        {
                            Component = "RBC",
                            LowValue = 4.5,
                            HighValue = 5.9,
                            Unit = "10^12/L",
                            Gender = "Male"
                        }
                    }
                },
                new LabTestDefinitionEntity
                {
                    Name = "Basic Metabolic Panel",
                    Code = "BMP",
                    Description = "Measures glucose, calcium, and various electrolytes",
                    Category = "Chemistry",
                    ReferenceRanges = new List<LabReferenceRangeEntity>
                    {
                        new LabReferenceRangeEntity
                        {
                            Component = "Glucose",
                            LowValue = 70,
                            HighValue = 100,
                            Unit = "mg/dL",
                            Gender = "All"
                        },
                        new LabReferenceRangeEntity
                        {
                            Component = "Calcium",
                            LowValue = 8.5,
                            HighValue = 10.5,
                            Unit = "mg/dL",
                            Gender = "All"
                        }
                    }
                }
            };

            await _context.LabTestDefinitions.AddRangeAsync(labTests);
        }

        private async Task SeedTimeSlots()
        {
            var today = DateTime.Today;
            var timeSlots = new List<TimeSlot>();

            // Create time slots for the next 7 days
            for (int day = 0; day < 7; day++)
            {
                var currentDate = today.AddDays(day);
                
                // Create slots from 9 AM to 5 PM
                for (int hour = 9; hour < 17; hour++)
                {
                    timeSlots.Add(new TimeSlot
                    {
                        StartTime = currentDate.AddHours(hour),
                        EndTime = currentDate.AddHours(hour + 1),
                        IsAvailable = true,
                        DoctorId = _context.Users
                            .First(u => u.UserRoles.Any(ur => ur.Role.Name == "Doctor")).Id
                    });
                }
            }

            await _context.TimeSlots.AddRangeAsync(timeSlots);
        }
    }
}
