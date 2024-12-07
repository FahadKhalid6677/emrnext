using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class TemplateField
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }

        // Field Configuration
        public string FieldType { get; set; } // Text, Number, Date, Select, etc.
        public string InputType { get; set; } // TextBox, TextArea, Dropdown, Radio, etc.
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
        
        // Data Configuration
        public string DataType { get; set; } // string, int, decimal, datetime, etc.
        public string Format { get; set; } // Date format, number format, etc.
        public string Unit { get; set; } // Medical units (mg, ml, etc.)
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        
        // Options Configuration
        public string Options { get; set; } // JSON array for dropdown/radio options
        public string OptionSource { get; set; } // Static, API, Database
        public string OptionSourceConfig { get; set; } // JSON configuration for dynamic options
        public bool AllowMultiple { get; set; }
        public bool AllowCustomOption { get; set; }
        
        // Validation
        public bool EnableValidation { get; set; }
        public string ValidationRules { get; set; } // JSON array of validation rules
        public string ValidationMessage { get; set; }
        public string RegexPattern { get; set; }
        
        // Clinical Integration
        public string HL7Field { get; set; }
        public string FHIRMapping { get; set; }
        public string SnowmedCode { get; set; }
        public string LoincCode { get; set; }
        
        // UI Configuration
        public string Style { get; set; } // JSON object for styling
        public string CssClass { get; set; }
        public bool IsVisible { get; set; }
        public string DependentFields { get; set; } // JSON array of dependent field IDs
        public string DisplayCondition { get; set; } // JSON logic for conditional display
        
        // Calculation
        public bool IsCalculated { get; set; }
        public string CalculationFormula { get; set; }
        public string DependentVariables { get; set; } // JSON array of variables used in calculation
        
        // Audit
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual TemplateSection Section { get; set; }
        public virtual ICollection<FieldHistory> History { get; set; }
        public virtual ICollection<FieldValue> Values { get; set; }
    }
}
