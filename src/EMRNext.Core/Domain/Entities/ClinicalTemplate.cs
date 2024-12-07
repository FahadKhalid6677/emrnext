using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class ClinicalTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SpecialtyType { get; set; }
        public string Purpose { get; set; } // History, Physical, Progress Note, etc.
        
        // Version Control
        public int Version { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        
        // Content Management
        public string HeaderContent { get; set; }
        public string FooterContent { get; set; }
        public bool RequiresSignature { get; set; }
        public bool RequiresCoSign { get; set; }
        
        // Integration
        public bool EnableDecisionSupport { get; set; }
        public string AssociatedRules { get; set; } // JSON array of rule IDs
        public string RequiredFields { get; set; } // JSON array of field definitions
        
        // Access Control
        public string AllowedRoles { get; set; } // JSON array of role IDs
        public string AllowedSpecialties { get; set; } // JSON array of specialty types
        public bool AllowSharing { get; set; }
        public bool AllowCustomization { get; set; }
        
        // Audit
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual ICollection<TemplateSection> Sections { get; set; }
        public virtual ICollection<TemplateVariable> Variables { get; set; }
        public virtual ICollection<TemplateUsage> UsageHistory { get; set; }
        public virtual ICollection<TemplateCustomization> Customizations { get; set; }
    }
}
