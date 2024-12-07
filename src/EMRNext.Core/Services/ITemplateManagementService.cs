using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services
{
    public interface ITemplateManagementService
    {
        // Template Management
        Task<ClinicalTemplate> CreateTemplateAsync(ClinicalTemplate template);
        Task<ClinicalTemplate> UpdateTemplateAsync(ClinicalTemplate template);
        Task<ClinicalTemplate> GetTemplateByIdAsync(int templateId);
        Task<IEnumerable<ClinicalTemplate>> GetTemplatesAsync(string category = null, string specialty = null);
        Task<bool> DeleteTemplateAsync(int templateId);
        Task<ClinicalTemplate> PublishTemplateAsync(int templateId);
        Task<ClinicalTemplate> CreateVersionAsync(int templateId);
        
        // Section Management
        Task<TemplateSection> AddSectionAsync(int templateId, TemplateSection section);
        Task<TemplateSection> UpdateSectionAsync(TemplateSection section);
        Task<bool> DeleteSectionAsync(int sectionId);
        Task<bool> ReorderSectionsAsync(int templateId, List<int> sectionIds);
        
        // Field Management
        Task<TemplateField> AddFieldAsync(int sectionId, TemplateField field);
        Task<TemplateField> UpdateFieldAsync(TemplateField field);
        Task<bool> DeleteFieldAsync(int fieldId);
        Task<bool> ReorderFieldsAsync(int sectionId, List<int> fieldIds);
        
        // Variable Management
        Task<TemplateVariable> AddVariableAsync(int templateId, TemplateVariable variable);
        Task<TemplateVariable> UpdateVariableAsync(TemplateVariable variable);
        Task<bool> DeleteVariableAsync(int variableId);
        Task<object> ResolveVariableAsync(int variableId, Dictionary<string, object> context);
        
        // Template Operations
        Task<Dictionary<string, object>> ValidateTemplateAsync(int templateId);
        Task<string> GeneratePreviewAsync(int templateId, Dictionary<string, object> data);
        Task<string> RenderTemplateAsync(int templateId, Dictionary<string, object> data);
        Task<bool> ImportTemplateAsync(string templateData);
        Task<string> ExportTemplateAsync(int templateId);
        
        // Access Control
        Task<bool> HasAccessAsync(int templateId, string userId, string action);
        Task<bool> ShareTemplateAsync(int templateId, List<string> userIds, string permission);
        Task<bool> RevokeAccessAsync(int templateId, string userId);
        
        // Usage Tracking
        Task LogUsageAsync(int templateId, string userId, string action);
        Task<IEnumerable<dynamic>> GetUsageStatisticsAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null);
        
        // Clinical Integration
        Task<IEnumerable<ClinicalTemplate>> GetTemplatesForEncounterAsync(int encounterId);
        Task<IEnumerable<ClinicalRule>> GetAssociatedRulesAsync(int templateId);
        Task<bool> AssociateRulesAsync(int templateId, List<int> ruleIds);
    }
}
