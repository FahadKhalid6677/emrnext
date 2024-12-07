using System;

namespace EMRNext.Core.Domain.Entities
{
    public class TemplateVariable
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        
        // Variable Configuration
        public string VariableType { get; set; } // Patient, Provider, Encounter, Custom
        public string DataType { get; set; } // string, int, decimal, datetime, etc.
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        
        // Data Source
        public string SourceType { get; set; } // Database, API, Function, Manual
        public string SourceConfig { get; set; } // JSON configuration for data source
        public string MappingPath { get; set; } // Path to data in source
        public bool AutoResolve { get; set; }
        
        // Format Configuration
        public string Format { get; set; } // Display format
        public string FormatConfig { get; set; } // JSON configuration for formatting
        public string Unit { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        
        // Validation
        public bool EnableValidation { get; set; }
        public string ValidationRules { get; set; } // JSON array of validation rules
        public string ValidationMessage { get; set; }
        
        // Clinical Integration
        public string HL7Field { get; set; }
        public string FHIRMapping { get; set; }
        public string SnowmedCode { get; set; }
        public string LoincCode { get; set; }
        
        // Cache Configuration
        public bool EnableCache { get; set; }
        public int? CacheDuration { get; set; }
        public DateTime? LastRefreshed { get; set; }
        
        // Security
        public bool IsEncrypted { get; set; }
        public string AccessLevel { get; set; }
        public string AllowedRoles { get; set; }
        
        // Audit
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual ClinicalTemplate Template { get; set; }
    }
}
