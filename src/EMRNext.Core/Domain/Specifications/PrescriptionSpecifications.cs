using System;
using System.Linq.Expressions;
using EMRNext.Core.Domain.Specifications;
using EMRNext.Core.Models;

namespace EMRNext.Core.Domain.Specifications
{
    /// <summary>
    /// Specifications for prescription queries
    /// </summary>
    public static class PrescriptionSpecifications
    {
        /// <summary>
        /// Specification for active patient prescriptions
        /// </summary>
        public class ActivePatientPrescriptionsSpec : BaseSpecification<Prescription>
        {
            public ActivePatientPrescriptionsSpec(int patientId) 
                : base(p => p.PatientId == patientId && 
                            p.Status == PrescriptionStatus.Active && 
                            p.ExpirationDate > DateTime.UtcNow)
            {
                AddInclude(p => p.Fills);
            }
        }

        /// <summary>
        /// Specification for prescriptions due for refill
        /// </summary>
        public class PrescriptionsDueForRefillSpec : BaseSpecification<Prescription>
        {
            public PrescriptionsDueForRefillSpec() 
                : base(p => p.Status == PrescriptionStatus.Active && 
                            p.RemainingRefills > 0 && 
                            p.ExpirationDate > DateTime.UtcNow)
            {
                AddInclude(p => p.Fills);
            }
        }

        /// <summary>
        /// Specification for expired prescriptions
        /// </summary>
        public class ExpiredPrescriptionsSpec : BaseSpecification<Prescription>
        {
            public ExpiredPrescriptionsSpec() 
                : base(p => p.Status == PrescriptionStatus.Active && 
                            p.ExpirationDate <= DateTime.UtcNow)
            {
                AddOrderByDescending(p => p.ExpirationDate);
            }
        }
    }
}
