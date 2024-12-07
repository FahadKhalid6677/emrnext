using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Clinical
{
    public class ExternalLabEntity : BaseIntEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string ContactPerson { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public string AccreditationNumber { get; set; }
        public DateTime? AccreditationExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string InterfaceType { get; set; }
        public string InterfaceSettings { get; set; }
        public string Notes { get; set; }
        public string ServiceLevel { get; set; }
        public string ContractStatus { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public string BillingInformation { get; set; }
        public virtual ICollection<LabOrderEntity> Orders { get; set; }
        public virtual ICollection<ExternalLabTest> AvailableTests { get; set; }
    }

    public class ExternalLabTest : BaseIntEntity
    {
        public int ExternalLabId { get; set; }
        public virtual ExternalLabEntity ExternalLab { get; set; }
        public int LabTestDefinitionId { get; set; }
        public virtual LabTestDefinitionEntity TestDefinition { get; set; }
        public string ExternalCode { get; set; }
        public decimal Cost { get; set; }
        public string TurnaroundTime { get; set; }
        public string SpecialRequirements { get; set; }
        public bool IsAvailable { get; set; }
        public string Notes { get; set; }
    }
}
