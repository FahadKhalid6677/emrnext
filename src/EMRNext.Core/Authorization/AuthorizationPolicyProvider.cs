using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EMRNext.Core.Authorization
{
    /// <summary>
    /// Custom authorization policy provider
    /// </summary>
    public class EMRNextAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly Dictionary<string, Func<AuthorizationHandlerContext, bool>> _policyRules;

        public EMRNextAuthorizationPolicyProvider()
        {
            _policyRules = new Dictionary<string, Func<AuthorizationHandlerContext, bool>>
            {
                { PolicyConstants.ViewPatientRecord, CanViewPatientRecord },
                { PolicyConstants.EditPatientRecord, CanEditPatientRecord },
                { PolicyConstants.SystemAdministration, IsSystemAdministrator }
            };
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.Role)
                .Build();

            return Task.FromResult(policy);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() 
            => Task.FromResult(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build()
            );

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() 
            => Task.FromResult<AuthorizationPolicy>(null);

        /// <summary>
        /// Check if user can view patient record
        /// </summary>
        private bool CanViewPatientRecord(AuthorizationHandlerContext context)
        {
            var user = context.User;
            return user.HasClaim(c => 
                c.Type == ClaimTypes.Role && 
                (c.Value == "Physician" || c.Value == "Nurse" || c.Value == "Administrator")
            );
        }

        /// <summary>
        /// Check if user can edit patient record
        /// </summary>
        private bool CanEditPatientRecord(AuthorizationHandlerContext context)
        {
            var user = context.User;
            return user.HasClaim(c => 
                c.Type == ClaimTypes.Role && 
                (c.Value == "Physician" || c.Value == "Administrator")
            );
        }

        /// <summary>
        /// Check if user is system administrator
        /// </summary>
        private bool IsSystemAdministrator(AuthorizationHandlerContext context)
        {
            var user = context.User;
            return user.HasClaim(c => 
                c.Type == ClaimTypes.Role && 
                c.Value == "Administrator"
            );
        }
    }

    /// <summary>
    /// Authorization handler for custom policies
    /// </summary>
    public class EMRNextAuthorizationHandler : AuthorizationHandler<IAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            IAuthorizationRequirement requirement)
        {
            // Custom authorization logic
            if (context.User.Identity.IsAuthenticated)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
