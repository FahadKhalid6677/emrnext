using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string TaxId { get; set; }
        public string TaxIdType { get; set; }
        public string NPI { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public bool IsServiceLocation { get; set; }
        public bool IsBillingLocation { get; set; }
        public string Color { get; set; } // For calendar display
        
        // Navigation Properties
        public virtual ICollection<Provider> Providers { get; set; }
        public virtual ICollection<Encounter> Encounters { get; set; }
        public virtual ICollection<Encounter> BillingEncounters { get; set; }
    }
}
