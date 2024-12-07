using System;
using System.Linq.Expressions;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Specifications;

namespace EMRNext.Core.Domain.Specifications
{
    /// <summary>
    /// Specifications for patient queries
    /// </summary>
    public class PatientSpecifications
    {
        /// <summary>
        /// Specification for patients within an age range
        /// </summary>
        public class PatientByAgeRangeSpec : BaseSpecification<Patient>
        {
            public PatientByAgeRangeSpec(int minAge, int maxAge) 
                : base(p => CalculateAge(p.DateOfBirth) >= minAge && 
                            CalculateAge(p.DateOfBirth) <= maxAge)
            {
                AddInclude(p => p.Vitals);
                ApplyOrderBy(p => p.LastName);
            }

            private static int CalculateAge(DateTime birthDate)
            {
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        /// <summary>
        /// Specification for patients by primary care provider
        /// </summary>
        public class PatientByProviderSpec : BaseSpecification<Patient>
        {
            public PatientByProviderSpec(string providerId) 
                : base(p => p.PrimaryCareProvider == providerId)
            {
                AddInclude(p => p.Vitals);
                AddInclude(p => p.Prescriptions);
                ApplyOrderBy(p => p.LastName);
            }
        }

        /// <summary>
        /// Specification for active patients
        /// </summary>
        public class ActivePatientsSpec : BaseSpecification<Patient>
        {
            public ActivePatientsSpec() 
                : base(p => !p.IsDeleted)
            {
                ApplyOrderByDescending(p => p.CreatedAt);
            }
        }

        /// <summary>
        /// Specification for patients with pending medical alerts
        /// </summary>
        public class PatientsWithPendingAlertsSpec : BaseSpecification<Patient>
        {
            public PatientsWithPendingAlertsSpec() 
                : base(p => p.Alerts.Any(a => !a.IsResolved))
            {
                AddInclude(p => p.Alerts);
                ApplyOrderBy(p => p.LastName);
            }
        }
    }
}
