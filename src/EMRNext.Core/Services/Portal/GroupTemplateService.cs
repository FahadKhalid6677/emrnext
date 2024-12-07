using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Services.Clinical;
using EMRNext.Core.Services.Document;
using EMRNext.Core.Services.Resource;

namespace EMRNext.Core.Services.Portal
{
    public class GroupTemplateService : IGroupTemplateService
    {
        private readonly EMRNextDbContext _context;
        private readonly IClinicalProtocolService _protocolService;
        private readonly IResourceManagementService _resourceService;
        private readonly IDocumentService _documentService;
        private readonly IQualityMeasureService _qualityService;
        private readonly IAuditService _auditService;
        private readonly IValidationService _validationService;

        public GroupTemplateService(
            EMRNextDbContext context,
            IClinicalProtocolService protocolService,
            IResourceManagementService resourceService,
            IDocumentService documentService,
            IQualityMeasureService qualityService,
            IAuditService auditService,
            IValidationService validationService)
        {
            _context = context;
            _protocolService = protocolService;
            _resourceService = resourceService;
            _documentService = documentService;
            _qualityService = qualityService;
            _auditService = auditService;
            _validationService = validationService;
        }

        public async Task<GroupSessionTemplate> CreateTemplateAsync(GroupSessionTemplate template)
        {
            // Validate clinical requirements
            var clinicalValidation = await _protocolService.ValidateProtocolRequirementsAsync(
                template.ClinicalProtocol,
                template.RequiredQualifications);

            if (!clinicalValidation.IsValid)
            {
                throw new ValidationException("Clinical requirements validation failed", 
                    clinicalValidation.Errors);
            }

            // Validate resource requirements
            var resourceValidation = await _resourceService.ValidateResourceRequirementsAsync(
                template.ResourceRequirements);

            if (!resourceValidation.IsValid)
            {
                throw new ValidationException("Resource requirements validation failed",
                    resourceValidation.Errors);
            }

            // Version management
            template.Version = await GetNextVersionNumberAsync(template.Name);
            template.CreatedDate = DateTime.UtcNow;
            template.IsActive = true;

            // Create template
            _context.GroupSessionTemplates.Add(template);
            
            // Initialize related components
            await InitializeMaterialTemplatesAsync(template);
            await InitializeDocumentationTemplatesAsync(template);
            await InitializeOutcomeMeasuresAsync(template);
            await InitializeFollowUpProtocolsAsync(template);

            await _context.SaveChangesAsync();

            // Audit trail
            await _auditService.LogActivityAsync(
                "TemplateCreation",
                $"Created template: {template.Name} v{template.Version}",
                template);

            return template;
        }

        public async Task<GroupSessionTemplate> GetTemplateAsync(int templateId)
        {
            return await _context.GroupSessionTemplates
                .Include(t => t.MaterialTemplates)
                .Include(t => t.DocumentationTemplates)
                .Include(t => t.OutcomeMeasures)
                .Include(t => t.FollowUpProtocols)
                .Include(t => t.ResourceRequirements)
                .FirstOrDefaultAsync(t => t.Id == templateId);
        }

        public async Task<IEnumerable<GroupSessionTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.GroupSessionTemplates
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.Version)
                .ToListAsync();
        }

        public async Task<GroupSessionTemplate> UpdateTemplateAsync(GroupSessionTemplate template)
        {
            var existingTemplate = await GetTemplateAsync(template.Id);
            if (existingTemplate == null)
            {
                throw new NotFoundException("Template not found");
            }

            // Create new version if significant changes
            if (HasSignificantChanges(existingTemplate, template))
            {
                return await CreateNewVersionAsync(template);
            }

            // Update existing version
            existingTemplate.LastModifiedDate = DateTime.UtcNow;
            _context.Entry(existingTemplate).CurrentValues.SetValues(template);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "TemplateUpdate",
                $"Updated template: {template.Name} v{template.Version}",
                template);

            return existingTemplate;
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                return false;
            }

            // Soft delete
            template.IsActive = false;
            template.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "TemplateDelete",
                $"Deactivated template: {template.Name} v{template.Version}",
                template);

            return true;
        }

        // Material Template Management
        public async Task<SessionMaterialTemplate> AddMaterialTemplateAsync(
            int templateId, 
            SessionMaterialTemplate material)
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                throw new NotFoundException("Template not found");
            }

            // Validate material content
            await _documentService.ValidateMaterialContentAsync(material);

            material.GroupSessionTemplateId = templateId;
            _context.SessionMaterialTemplates.Add(material);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "MaterialTemplateAdd",
                $"Added material template: {material.Name} to {template.Name}",
                material);

            return material;
        }

        // Documentation Template Management
        public async Task<DocumentationTemplate> AddDocumentationTemplateAsync(
            int templateId, 
            DocumentationTemplate documentation)
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                throw new NotFoundException("Template not found");
            }

            // Validate documentation template
            await _documentService.ValidateDocumentationTemplateAsync(documentation);

            documentation.GroupSessionTemplateId = templateId;
            _context.DocumentationTemplates.Add(documentation);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "DocumentationTemplateAdd",
                $"Added documentation template: {documentation.Name} to {template.Name}",
                documentation);

            return documentation;
        }

        // Outcome Measure Management
        public async Task<OutcomeMeasureTemplate> AddOutcomeMeasureAsync(
            int templateId, 
            OutcomeMeasureTemplate measure)
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                throw new NotFoundException("Template not found");
            }

            // Validate outcome measure
            await _qualityService.ValidateOutcomeMeasureAsync(measure);

            measure.GroupSessionTemplateId = templateId;
            _context.OutcomeMeasureTemplates.Add(measure);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "OutcomeMeasureAdd",
                $"Added outcome measure: {measure.Name} to {template.Name}",
                measure);

            return measure;
        }

        // Follow-up Protocol Management
        public async Task<FollowUpProtocol> AddFollowUpProtocolAsync(
            int templateId, 
            FollowUpProtocol protocol)
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                throw new NotFoundException("Template not found");
            }

            // Validate follow-up protocol
            await _protocolService.ValidateFollowUpProtocolAsync(protocol);

            protocol.GroupSessionTemplateId = templateId;
            _context.FollowUpProtocols.Add(protocol);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "FollowUpProtocolAdd",
                $"Added follow-up protocol: {protocol.Name} to {template.Name}",
                protocol);

            return protocol;
        }

        // Template Application
        public async Task<GroupAppointment> ApplyTemplateToSessionAsync(
            int templateId, 
            DateTime sessionDate)
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                throw new NotFoundException("Template not found");
            }

            var appointment = new GroupAppointment
            {
                Name = template.Name,
                Description = template.Description,
                AppointmentTypeId = template.AppointmentTypeId,
                ProviderId = template.DefaultProviderId,
                LocationId = template.DefaultLocationId,
                StartTime = sessionDate,
                EndTime = sessionDate.AddMinutes(template.DefaultDurationMinutes),
                MaxParticipants = template.MaxParticipants,
                MinParticipants = template.MinParticipants,
                Status = "Scheduled",
                CreatedDate = DateTime.UtcNow
            };

            // Apply template components
            await ApplyMaterialTemplatesAsync(template, appointment);
            await ApplyDocumentationTemplatesAsync(template, appointment);
            await ApplyOutcomeMeasuresAsync(template, appointment);
            await ApplyFollowUpProtocolsAsync(template, appointment);
            await ApplyResourceRequirementsAsync(template, appointment);

            return appointment;
        }

        private async Task<int> GetNextVersionNumberAsync(string templateName)
        {
            var latestVersion = await _context.GroupSessionTemplates
                .Where(t => t.Name == templateName)
                .MaxAsync(t => (int?)t.Version) ?? 0;
            return latestVersion + 1;
        }

        private bool HasSignificantChanges(
            GroupSessionTemplate existing, 
            GroupSessionTemplate updated)
        {
            // Compare significant fields to determine if new version is needed
            return existing.ClinicalProtocol != updated.ClinicalProtocol ||
                   existing.RequiredQualifications != updated.RequiredQualifications ||
                   existing.AppointmentTypeId != updated.AppointmentTypeId ||
                   existing.DefaultDurationMinutes != updated.DefaultDurationMinutes;
        }

        private async Task<GroupSessionTemplate> CreateNewVersionAsync(
            GroupSessionTemplate template)
        {
            template.Id = 0; // Clear ID for new version
            template.Version = await GetNextVersionNumberAsync(template.Name);
            template.CreatedDate = DateTime.UtcNow;
            template.LastModifiedDate = null;

            _context.GroupSessionTemplates.Add(template);
            await _context.SaveChangesAsync();

            return template;
        }

        private async Task InitializeMaterialTemplatesAsync(GroupSessionTemplate template)
        {
            if (template.MaterialTemplates == null) return;

            foreach (var material in template.MaterialTemplates)
            {
                await _documentService.ValidateMaterialContentAsync(material);
                material.GroupSessionTemplateId = template.Id;
            }
        }

        private async Task InitializeDocumentationTemplatesAsync(GroupSessionTemplate template)
        {
            if (template.DocumentationTemplates == null) return;

            foreach (var doc in template.DocumentationTemplates)
            {
                await _documentService.ValidateDocumentationTemplateAsync(doc);
                doc.GroupSessionTemplateId = template.Id;
            }
        }

        private async Task InitializeOutcomeMeasuresAsync(GroupSessionTemplate template)
        {
            if (template.OutcomeMeasures == null) return;

            foreach (var measure in template.OutcomeMeasures)
            {
                await _qualityService.ValidateOutcomeMeasureAsync(measure);
                measure.GroupSessionTemplateId = template.Id;
            }
        }

        private async Task InitializeFollowUpProtocolsAsync(GroupSessionTemplate template)
        {
            if (template.FollowUpProtocols == null) return;

            foreach (var protocol in template.FollowUpProtocols)
            {
                await _protocolService.ValidateFollowUpProtocolAsync(protocol);
                protocol.GroupSessionTemplateId = template.Id;
            }
        }

        private async Task ApplyMaterialTemplatesAsync(
            GroupSessionTemplate template, 
            GroupAppointment appointment)
        {
            foreach (var materialTemplate in template.MaterialTemplates)
            {
                var material = await _documentService.CreateSessionMaterialAsync(
                    materialTemplate,
                    appointment);
                appointment.Materials.Add(material);
            }
        }

        private async Task ApplyDocumentationTemplatesAsync(
            GroupSessionTemplate template, 
            GroupAppointment appointment)
        {
            foreach (var docTemplate in template.DocumentationTemplates)
            {
                var documentation = await _documentService.CreateSessionDocumentationAsync(
                    docTemplate,
                    appointment);
                appointment.Documentation.Add(documentation);
            }
        }

        private async Task ApplyOutcomeMeasuresAsync(
            GroupSessionTemplate template, 
            GroupAppointment appointment)
        {
            foreach (var measureTemplate in template.OutcomeMeasures)
            {
                var measure = await _qualityService.CreateSessionOutcomeMeasureAsync(
                    measureTemplate,
                    appointment);
                appointment.OutcomeMeasures.Add(measure);
            }
        }

        private async Task ApplyFollowUpProtocolsAsync(
            GroupSessionTemplate template, 
            GroupAppointment appointment)
        {
            foreach (var protocolTemplate in template.FollowUpProtocols)
            {
                var protocol = await _protocolService.CreateSessionFollowUpProtocolAsync(
                    protocolTemplate,
                    appointment);
                appointment.FollowUpProtocols.Add(protocol);
            }
        }

        private async Task ApplyResourceRequirementsAsync(
            GroupSessionTemplate template, 
            GroupAppointment appointment)
        {
            foreach (var requirement in template.ResourceRequirements)
            {
                var resource = await _resourceService.AllocateResourceAsync(
                    requirement,
                    appointment);
                appointment.Resources.Add(resource);
            }
        }
    }
}
