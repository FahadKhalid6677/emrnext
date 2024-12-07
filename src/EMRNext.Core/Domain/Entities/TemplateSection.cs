using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class TemplateSection
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        
        // Content Management
        public string Content { get; set; }
        public string ContentType { get; set; } // RichText, Form, Table, etc.
        public bool IsRequired { get; set; }
        public bool IsRepeatable { get; set; }
        public int? MaxRepetitions { get; set; }
        
        // Conditional Display
        public bool HasConditions { get; set; }
        public string DisplayConditions { get; set; } // JSON logic for conditional display
        public string DependentSections { get; set; } // JSON array of dependent section IDs
        
        // Data Binding
        public string DataSource { get; set; } // JSON configuration for data source
        public string DataMapping { get; set; } // JSON mapping of fields to data
        public bool AutoPopulate { get; set; }
        public string DefaultValues { get; set; } // JSON object of default values
        
        // Validation
        public bool EnableValidation { get; set; }
        public string ValidationRules { get; set; } // JSON array of validation rules
        public string ValidationMessage { get; set; }
        
        // UI Configuration
        public string Style { get; set; } // JSON object for styling
        public string Layout { get; set; } // JSON object for layout configuration
        public bool IsCollapsible { get; set; }
        public bool IsCollapsedByDefault { get; set; }
        
        // Access Control
        public string EditRoles { get; set; } // JSON array of role IDs
        public string ViewRoles { get; set; } // JSON array of role IDs
        public bool IsReadOnly { get; set; }
        
        // Audit
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual ClinicalTemplate Template { get; set; }
        public virtual ICollection<TemplateField> Fields { get; set; }
        public virtual ICollection<SectionHistory> History { get; set; }
    }
}
