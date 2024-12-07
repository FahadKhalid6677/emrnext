using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Insurance
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Type { get; set; } // Primary, Secondary, Tertiary
        public DateTime Date { get; set; }
        public string Provider { get; set; }
        public string PolicyNumber { get; set; }
        public string GroupNumber { get; set; }
        public string PlanName { get; set; }
        public string SubscriberFirstName { get; set; }
        public string SubscriberLastName { get; set; }
        public string SubscriberSocialSecurityNumber { get; set; }
        public DateTime? SubscriberDateOfBirth { get; set; }
        public string SubscriberPhone { get; set; }
        public string SubscriberEmployer { get; set; }
        public string SubscriberEmployerCity { get; set; }
        public string SubscriberEmployerState { get; set; }
        public string SubscriberEmployerPostalCode { get; set; }
        public string RelationshipToSubscriber { get; set; }
        public string CopayAmount { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool AcceptAssignment { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation Properties
        public virtual Patient Patient { get; set; }
    }
}
