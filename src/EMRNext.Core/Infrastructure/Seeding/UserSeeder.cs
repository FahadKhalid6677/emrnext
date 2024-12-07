using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Infrastructure.Persistence;
using EMRNext.Core.Identity;

namespace EMRNext.Core.Infrastructure.Seeding
{
    /// <summary>
    /// Seed initial users and roles for the EMRNext application
    /// </summary>
    public class UserSeeder : BaseSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserSeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserSeeder> logger) : base(logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        /// <summary>
        /// Priority of seeding (lower number = earlier execution)
        /// </summary>
        public override int Priority => 1;

        /// <summary>
        /// Seed initial users and roles
        /// </summary>
        public override async Task SeedAsync(EMRNextDbContext context)
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedDefaultUsersAsync();
        }

        /// <summary>
        /// Create initial application roles
        /// </summary>
        private async Task SeedRolesAsync()
        {
            string[] roleNames = { 
                "Administrator", 
                "Physician", 
                "Nurse", 
                "Patient", 
                "Researcher" 
            };

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    await _roleManager.CreateAsync(role);
                    _logger.LogInformation($"Created role: {roleName}");
                }
            }
        }

        /// <summary>
        /// Create the initial admin user
        /// </summary>
        private async Task SeedAdminUserAsync()
        {
            var adminEmail = "admin@emrnext.com";
            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "EMRNext",
                    LastName = "Administrator",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(
                    adminUser, 
                    "Admin@123!EMRNext"  // Strong initial password
                );

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Administrator");
                    _logger.LogInformation("Created admin user");
                }
                else
                {
                    _logger.LogError("Failed to create admin user");
                }
            }
        }

        /// <summary>
        /// Create default users for testing and initial setup
        /// </summary>
        private async Task SeedDefaultUsersAsync()
        {
            var defaultUsers = new[]
            {
                new { 
                    Email = "physician@emrnext.com", 
                    Role = "Physician", 
                    FirstName = "Dr. Jane", 
                    LastName = "Smith" 
                },
                new { 
                    Email = "nurse@emrnext.com", 
                    Role = "Nurse", 
                    FirstName = "Emily", 
                    LastName = "Johnson" 
                },
                new { 
                    Email = "researcher@emrnext.com", 
                    Role = "Researcher", 
                    FirstName = "Alex", 
                    LastName = "Research" 
                }
            };

            foreach (var userInfo in defaultUsers)
            {
                var existingUser = await _userManager.FindByEmailAsync(userInfo.Email);

                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        EmailConfirmed = true,
                        FirstName = userInfo.FirstName,
                        LastName = userInfo.LastName,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(
                        user, 
                        $"Test@123!{userInfo.Role}"  // Strong initial password
                    );

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, userInfo.Role);
                        _logger.LogInformation($"Created {userInfo.Role} user: {userInfo.Email}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to create {userInfo.Role} user: {userInfo.Email}");
                    }
                }
            }
        }
    }
}
