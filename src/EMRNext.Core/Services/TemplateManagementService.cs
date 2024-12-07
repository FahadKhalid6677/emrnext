using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Services.Security;
using EMRNext.Core.Services.Clinical;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EMRNext.Core.Services
{
    public class TemplateManagementService : ITemplateManagementService
    {
        private readonly EMRDbContext _context;
        private readonly ISecurityService _securityService;
        private readonly IClinicalDecisionSupportService _clinicalService;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public TemplateManagementService(
            EMRDbContext context,
            ISecurityService securityService,
            IClinicalDecisionSupportService clinicalService,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _context = context;
            _securityService = securityService;
            _clinicalService = clinicalService;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        // Template Management
        public async Task<ClinicalTemplate> CreateTemplateAsync(ClinicalTemplate template)
        {
            await ValidateTemplateStructureAsync(template);
            
            template.CreatedAt = DateTime.UtcNow;
            template.Version = 1;
            template.IsActive = true;
            
            _context.ClinicalTemplates.Add(template);
            await _context.SaveChangesAsync();
            
            await _auditService.LogActivityAsync("Template", template.Id, "Create", template.CreatedBy);
            return template;
        }

        public async Task<ClinicalTemplate> UpdateTemplateAsync(ClinicalTemplate template)
        {
            var existing = await _context.ClinicalTemplates
                .Include(t => t.Sections)
                .Include(t => t.Variables)
                .FirstOrDefaultAsync(t => t.Id == template.Id);

            if (existing == null)
                throw new NotFoundException("Template not found");

            if (!await HasAccessAsync(template.Id, template.ModifiedBy, "edit"))
                throw new UnauthorizedException("Insufficient permissions");

            // Create new version if published
            if (existing.IsPublished)
            {
                return await CreateVersionAsync(template.Id);
            }

            await ValidateTemplateStructureAsync(template);
            
            existing.Name = template.Name;
            existing.Description = template.Description;
            existing.Category = template.Category;
            existing.SpecialtyType = template.SpecialtyType;
            existing.Purpose = template.Purpose;
            existing.ModifiedAt = DateTime.UtcNow;
            existing.ModifiedBy = template.ModifiedBy;
            
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("Template", template.Id, "Update", template.ModifiedBy);
            
            return existing;
        }

        public async Task<ClinicalTemplate> GetTemplateByIdAsync(int templateId)
        {
            var template = await _context.ClinicalTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(t => t.Variables)
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                throw new NotFoundException("Template not found");

            return template;
        }

        public async Task<IEnumerable<ClinicalTemplate>> GetTemplatesAsync(string category = null, string specialty = null)
        {
            var query = _context.ClinicalTemplates
                .Include(t => t.Sections)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => t.Category == category);

            if (!string.IsNullOrEmpty(specialty))
                query = query.Where(t => t.SpecialtyType == specialty);

            return await query.ToListAsync();
        }

        public async Task<ClinicalTemplate> PublishTemplateAsync(int templateId)
        {
            var template = await GetTemplateByIdAsync(templateId);
            
            var validationResults = await ValidateTemplateAsync(templateId);
            if (validationResults.Any())
                throw new ValidationException("Template validation failed", validationResults);

            template.IsPublished = true;
            template.PublishedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync("Template", templateId, "Publish", template.ModifiedBy);
            
            return template;
        }

        // Section Management
        public async Task<TemplateSection> AddSectionAsync(int templateId, TemplateSection section)
        {
            var template = await GetTemplateByIdAsync(templateId);
            
            section.TemplateId = templateId;
            section.CreatedAt = DateTime.UtcNow;
            
            _context.TemplateSections.Add(section);
            await _context.SaveChangesAsync();
            
            await _auditService.LogActivityAsync("Section", section.Id, "Create", section.CreatedBy);
            return section;
        }

        // Clinical Integration
        public async Task<IEnumerable<ClinicalTemplate>> GetTemplatesForEncounterAsync(int encounterId)
        {
            var encounter = await _context.Encounters
                .Include(e => e.Patient)
                .Include(e => e.Provider)
                .FirstOrDefaultAsync(e => e.Id == encounterId);

            if (encounter == null)
                throw new NotFoundException("Encounter not found");

            // Get applicable templates based on encounter context
            var templates = await _context.ClinicalTemplates
                .Include(t => t.Sections)
                .Where(t => t.IsPublished && t.IsActive)
                .Where(t => t.SpecialtyType == encounter.Provider.Specialty)
                .ToListAsync();

            // Apply clinical decision support rules
            var applicableTemplates = new List<ClinicalTemplate>();
            foreach (var template in templates)
            {
                if (await EvaluateTemplateRulesAsync(template, encounter))
                {
                    applicableTemplates.Add(template);
                }
            }

            return applicableTemplates;
        }

        public async Task<string> RenderTemplateAsync(int templateId, Dictionary<string, object> data)
        {
            var template = await GetTemplateByIdAsync(templateId);
            var renderedContent = new System.Text.StringBuilder();

            foreach (var section in template.Sections.OrderBy(s => s.OrderIndex))
            {
                if (await ShouldRenderSectionAsync(section, data))
                {
                    var sectionContent = await RenderSectionAsync(section, data);
                    renderedContent.AppendLine(sectionContent);
                }
            }

            return renderedContent.ToString();
        }

        // Private Helper Methods
        private async Task ValidateTemplateStructureAsync(ClinicalTemplate template)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(template.Name))
                errors.Add("Template name is required");

            if (template.Sections != null)
            {
                foreach (var section in template.Sections)
                {
                    if (string.IsNullOrEmpty(section.Name))
                        errors.Add($"Section name is required for section {section.OrderIndex}");

                    if (section.Fields != null)
                    {
                        foreach (var field in section.Fields)
                        {
                            if (string.IsNullOrEmpty(field.Name))
                                errors.Add($"Field name is required in section {section.Name}");
                        }
                    }
                }
            }

            if (errors.Any())
                throw new ValidationException("Template validation failed", errors);
        }

        private async Task<bool> EvaluateTemplateRulesAsync(ClinicalTemplate template, Encounter encounter)
        {
            if (!template.EnableDecisionSupport)
                return true;

            var ruleIds = JsonConvert.DeserializeObject<List<int>>(template.AssociatedRules);
            foreach (var ruleId in ruleIds)
            {
                var evaluation = await _clinicalService.EvaluateRuleAsync(ruleId, encounter.Id);
                if (!evaluation.IsValid)
                    return false;
            }

            return true;
        }

        private async Task<bool> ShouldRenderSectionAsync(TemplateSection section, Dictionary<string, object> data)
        {
            if (!section.HasConditions)
                return true;

            var conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(section.DisplayConditions);
            return await EvaluateConditionsAsync(conditions, data);
        }

        private async Task<string> RenderSectionAsync(TemplateSection section, Dictionary<string, object> data)
        {
            var content = section.Content;

            // Replace variables
            var variablePattern = @"\{\{(.*?)\}\}";
            content = await Regex.Replace(content, variablePattern, async match =>
            {
                var variableName = match.Groups[1].Value.Trim();
                return await ResolveVariableValueAsync(variableName, data);
            });

            return content;
        }

        private async Task<string> ResolveVariableValueAsync(string variableName, Dictionary<string, object> data)
        {
            if (data.ContainsKey(variableName))
                return data[variableName]?.ToString();

            var variable = await _context.TemplateVariables
                .FirstOrDefaultAsync(v => v.Name == variableName);

            if (variable == null)
                return string.Empty;

            return await ResolveVariableAsync(variable.Id, data);
        }

        private async Task<bool> EvaluateConditionsAsync(Dictionary<string, object> conditions, Dictionary<string, object> data)
        {
            // Implement condition evaluation logic
            return true; // Placeholder
        }
    }
}
