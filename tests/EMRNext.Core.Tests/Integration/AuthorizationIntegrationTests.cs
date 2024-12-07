using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EMRNext.Core.Authorization;

namespace EMRNext.Core.Tests.Integration
{
    /// <summary>
    /// Integration tests for Authorization Policies
    /// </summary>
    public class AuthorizationIntegrationTests
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationIntegrationTests()
        {
            var services = new ServiceCollection();

            // Configure authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyConstants.ViewPatientRecord, 
                    policy => policy.RequireClaim(ClaimTypes.Role, "Physician", "Nurse", "Administrator"));
                
                options.AddPolicy(PolicyConstants.EditPatientRecord, 
                    policy => policy.RequireClaim(ClaimTypes.Role, "Physician", "Administrator"));
            });

            // Add custom authorization handler
            services.AddSingleton<IAuthorizationHandler, EMRNextAuthorizationHandler>();

            var serviceProvider = services.BuildServiceProvider();
            _authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
        }

        [Fact]
        public async Task Physician_ShouldHaveViewPatientRecordAccess()
        {
            // Arrange
            var user = CreateClaimsPrincipal("Physician");

            // Act
            var result = await _authorizationService.AuthorizeAsync(
                user, 
                PolicyConstants.ViewPatientRecord
            );

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task Nurse_ShouldHaveViewPatientRecordAccess()
        {
            // Arrange
            var user = CreateClaimsPrincipal("Nurse");

            // Act
            var result = await _authorizationService.AuthorizeAsync(
                user, 
                PolicyConstants.ViewPatientRecord
            );

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task Patient_ShouldNotHaveViewPatientRecordAccess()
        {
            // Arrange
            var user = CreateClaimsPrincipal("Patient");

            // Act
            var result = await _authorizationService.AuthorizeAsync(
                user, 
                PolicyConstants.ViewPatientRecord
            );

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Patient_ShouldNotHaveEditPatientRecordAccess()
        {
            // Arrange
            var user = CreateClaimsPrincipal("Patient");

            // Act
            var result = await _authorizationService.AuthorizeAsync(
                user, 
                PolicyConstants.EditPatientRecord
            );

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Administrator_ShouldHaveFullAccess()
        {
            // Arrange
            var user = CreateClaimsPrincipal("Administrator");

            // Act
            var viewResult = await _authorizationService.AuthorizeAsync(
                user, 
                PolicyConstants.ViewPatientRecord
            );
            var editResult = await _authorizationService.AuthorizeAsync(
                user, 
                PolicyConstants.EditPatientRecord
            );

            // Assert
            Assert.True(viewResult.Succeeded);
            Assert.True(editResult.Succeeded);
        }

        private ClaimsPrincipal CreateClaimsPrincipal(string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

            var identity = new ClaimsIdentity(claims, "TestAuthentication");
            return new ClaimsPrincipal(identity);
        }
    }
}
