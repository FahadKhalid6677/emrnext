using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.Portal
{
    public interface IGroupTemplateService
    {
        Task<GroupSessionTemplate> CreateTemplateAsync(GroupSessionTemplate template);
        Task<GroupSessionTemplate> GetTemplateAsync(int templateId);
        Task<IEnumerable<GroupSessionTemplate>> GetActiveTemplatesAsync();
        Task<GroupSessionTemplate> UpdateTemplateAsync(GroupSessionTemplate template);
        Task<bool> DeleteTemplateAsync(int templateId);
        
        // Material Management
        Task<SessionMaterialTemplate> AddMaterialTemplateAsync(int templateId, SessionMaterialTemplate material);
        Task<bool> UpdateMaterialTemplateAsync(SessionMaterialTemplate material);
        Task<bool> DeleteMaterialTemplateAsync(int materialId);
        
        // Documentation Templates
        Task<DocumentationTemplate> AddDocumentationTemplateAsync(int templateId, DocumentationTemplate documentation);
        Task<bool> UpdateDocumentationTemplateAsync(DocumentationTemplate documentation);
        Task<bool> DeleteDocumentationTemplateAsync(int documentationId);
        
        // Outcome Measures
        Task<OutcomeMeasureTemplate> AddOutcomeMeasureAsync(int templateId, OutcomeMeasureTemplate measure);
        Task<bool> UpdateOutcomeMeasureAsync(OutcomeMeasureTemplate measure);
        Task<bool> DeleteOutcomeMeasureAsync(int measureId);
        
        // Follow-up Protocols
        Task<FollowUpProtocol> AddFollowUpProtocolAsync(int templateId, FollowUpProtocol protocol);
        Task<bool> UpdateFollowUpProtocolAsync(FollowUpProtocol protocol);
        Task<bool> DeleteFollowUpProtocolAsync(int protocolId);
        
        // Template Application
        Task<GroupAppointment> ApplyTemplateToSessionAsync(int templateId, DateTime sessionDate);
        Task<GroupSeries> ApplyTemplateToSeriesAsync(int templateId, GroupSeries seriesConfig);
    }
}
