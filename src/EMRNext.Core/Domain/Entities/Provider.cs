using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class Provider
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NPI { get; set; }
        public string Specialty { get; set; }
        public string BillingName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public int? FacilityId { get; set; }
        public bool IsActive { get; set; }
        public bool IsAuthorized { get; set; }
        public bool CanSeeCalendar { get; set; }
        
        // Navigation Properties
        public virtual Facility Facility { get; set; }
        public virtual ICollection<Encounter> Encounters { get; set; }
        public virtual ICollection<Encounter> SupervisedEncounters { get; set; }
    }
}
