using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class GroupSessionTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AppointmentTypeId { get; set; }
        public virtual AppointmentType AppointmentType { get; set; }
        public int DefaultProviderId { get; set; }
        public virtual Provider DefaultProvider { get; set; }
        public int DefaultLocationId { get; set; }
        public virtual Location DefaultLocation { get; set; }
        public int DefaultDurationMinutes { get; set; }
        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public string ClinicalProtocol { get; set; }
        public string RequiredQualifications { get; set; }
        public bool RequiresPreScreening { get; set; }
        
        // Material Templates
        public virtual ICollection<SessionMaterialTemplate> MaterialTemplates { get; set; }
        
        // Documentation Templates
        public virtual ICollection<DocumentationTemplate> DocumentationTemplates { get; set; }
        
        // Outcome Measures
        public virtual ICollection<OutcomeMeasureTemplate> OutcomeMeasures { get; set; }
        
        // Follow-up Protocols
        public virtual ICollection<FollowUpProtocol> FollowUpProtocols { get; set; }
        
        // Resource Requirements
        public virtual ICollection<ResourceRequirement> ResourceRequirements { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public bool IsActive { get; set; }
    }

    public class SessionMaterialTemplate
    {
        public int Id { get; set; }
        public int GroupSessionTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContentType { get; set; }
        public string StoragePath { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public virtual GroupSessionTemplate GroupSessionTemplate { get; set; }
    }

    public class DocumentationTemplate
    {
        public int Id { get; set; }
        public int GroupSessionTemplateId { get; set; }
        public string Name { get; set; }
        public string Template { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public virtual GroupSessionTemplate GroupSessionTemplate { get; set; }
    }

    public class OutcomeMeasureTemplate
    {
        public int Id { get; set; }
        public int GroupSessionTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MeasureType { get; set; }
        public string MetricDefinition { get; set; }
        public string CollectionFrequency { get; set; }
        public bool IsRequired { get; set; }
        public virtual GroupSessionTemplate GroupSessionTemplate { get; set; }
    }

    public class FollowUpProtocol
    {
        public int Id { get; set; }
        public int GroupSessionTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TimeframeInDays { get; set; }
        public string ActionType { get; set; }
        public string Protocol { get; set; }
        public bool IsRequired { get; set; }
        public virtual GroupSessionTemplate GroupSessionTemplate { get; set; }
    }
}
