using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMRNext.Core.Domain.Entities
{
    /// <summary>
    /// Represents a medical note template for standardized documentation
    /// </summary>
    public class NoteTemplate : BaseIntEntity
    {
        [Required]
        [StringLength(200)]
        public string TemplateName { get; set; }

        [Required]
        [StringLength(100)]
        public string MedicalSpecialty { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }

        public List<NoteTemplateSection> Sections { get; set; } = new List<NoteTemplateSection>();

        /// <summary>
        /// Validates the entire note template structure
        /// </summary>
        public bool Validate()
        {
            // Ensure template has at least one section
            if (Sections == null || Sections.Count == 0)
                return false;

            // Validate each section
            foreach (var section in Sections)
            {
                if (!section.Validate())
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents a section within a note template
    /// </summary>
    public class NoteTemplateSection : BaseIntEntity
    {
        public Guid NoteTemplateId { get; set; }
        public NoteTemplate NoteTemplate { get; set; }

        [Required]
        [StringLength(200)]
        public string SectionName { get; set; }

        [StringLength(500)]
        public string SectionDescription { get; set; }

        public int DisplayOrder { get; set; }

        public List<NoteTemplateSectionField> Fields { get; set; } = new List<NoteTemplateSectionField>();

        /// <summary>
        /// Validates the section structure
        /// </summary>
        public bool Validate()
        {
            // Ensure section has a name
            if (string.IsNullOrWhiteSpace(SectionName))
                return false;

            // Validate each field
            foreach (var field in Fields)
            {
                if (!field.Validate())
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents a field within a note template section
    /// </summary>
    public class NoteTemplateSectionField : BaseIntEntity
    {
        public Guid NoteTemplateSectionId { get; set; }
        public NoteTemplateSection NoteTemplateSection { get; set; }

        [Required]
        [StringLength(200)]
        public string FieldName { get; set; }

        [Required]
        [StringLength(50)]
        public string FieldType { get; set; } // Text, Number, Date, Dropdown, etc.

        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }

        [StringLength(500)]
        public string Placeholder { get; set; }

        public string ValidationRegex { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }

        public List<string> DropdownOptions { get; set; } = new List<string>();

        /// <summary>
        /// Validates the field structure
        /// </summary>
        public bool Validate()
        {
            // Ensure field has a name and type
            if (string.IsNullOrWhiteSpace(FieldName) || string.IsNullOrWhiteSpace(FieldType))
                return false;

            // Validate length constraints
            if (MinLength.HasValue && MaxLength.HasValue && MinLength > MaxLength)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Represents an instance of a filled-out note template
    /// </summary>
    public class NoteTemplateInstance : BaseIntEntity
    {
        public Guid NoteTemplateId { get; set; }
        public NoteTemplate NoteTemplate { get; set; }

        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }

        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }

        public List<NoteTemplateInstanceField> FilledFields { get; set; } = new List<NoteTemplateInstanceField>();

        /// <summary>
        /// Validates the entire note template instance
        /// </summary>
        public bool Validate()
        {
            // Ensure all required fields are filled
            var template = NoteTemplate;
            foreach (var section in template.Sections)
            {
                foreach (var field in section.Fields)
                {
                    if (field.IsRequired)
                    {
                        var filledField = FilledFields.FirstOrDefault(f => f.NoteTemplateSectionFieldId == field.Id);
                        if (filledField == null || string.IsNullOrWhiteSpace(filledField.FieldValue))
                            return false;
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Represents a filled field in a note template instance
    /// </summary>
    public class NoteTemplateInstanceField : BaseIntEntity
    {
        public Guid NoteTemplateInstanceId { get; set; }
        public NoteTemplateInstance NoteTemplateInstance { get; set; }

        public Guid NoteTemplateSectionFieldId { get; set; }
        public NoteTemplateSectionField NoteTemplateSectionField { get; set; }

        public string FieldValue { get; set; }

        /// <summary>
        /// Validates the field value against the field's constraints
        /// </summary>
        public bool Validate()
        {
            var field = NoteTemplateSectionField;

            // Check if required field is empty
            if (field.IsRequired && string.IsNullOrWhiteSpace(FieldValue))
                return false;

            // Validate against regex if provided
            if (!string.IsNullOrWhiteSpace(field.ValidationRegex) && 
                !System.Text.RegularExpressions.Regex.IsMatch(FieldValue, field.ValidationRegex))
                return false;

            // Validate length constraints
            if (field.MinLength.HasValue && FieldValue.Length < field.MinLength)
                return false;

            if (field.MaxLength.HasValue && FieldValue.Length > field.MaxLength)
                return false;

            return true;
        }
    }
}
