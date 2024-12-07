using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;

namespace EMRNext.Core.Services.EhrServices
{
    public class NoteTemplateService : INoteTemplateService
    {
        private readonly ILogger<NoteTemplateService> _logger;
        private readonly IGenericRepository<NoteTemplate> _templateRepository;
        private readonly IGenericRepository<NoteTemplateInstance> _instanceRepository;
        private readonly IGenericRepository<Patient> _patientRepository;
        private readonly IGenericRepository<Provider> _providerRepository;

        public NoteTemplateService(
            ILogger<NoteTemplateService> logger,
            IGenericRepository<NoteTemplate> templateRepository,
            IGenericRepository<NoteTemplateInstance> instanceRepository,
            IGenericRepository<Patient> patientRepository,
            IGenericRepository<Provider> providerRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _instanceRepository = instanceRepository ?? throw new ArgumentNullException(nameof(instanceRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        }

        public async Task<NoteTemplate> CreateNoteTemplateAsync(NoteTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            if (!template.Validate())
            {
                _logger.LogWarning("Invalid note template structure");
                throw new ArgumentException("Invalid note template structure");
            }

            await _templateRepository.AddAsync(template);
            return template;
        }

        public async Task<NoteTemplate> GetNoteTemplateByIdAsync(Guid templateId)
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            
            if (template == null)
            {
                _logger.LogWarning($"Note template with ID {templateId} not found");
                throw new KeyNotFoundException($"Note template with ID {templateId} not found");
            }

            return template;
        }

        public async Task<IEnumerable<NoteTemplate>> GetNoteTemplatesBySpecialtyAsync(string medicalSpecialty)
        {
            if (string.IsNullOrWhiteSpace(medicalSpecialty))
                throw new ArgumentException("Medical specialty cannot be empty", nameof(medicalSpecialty));

            return await _templateRepository.FindAsync(
                template => template.MedicalSpecialty.ToLower() == medicalSpecialty.ToLower() && template.IsActive
            );
        }

        public async Task<NoteTemplateInstance> CreateNoteTemplateInstanceAsync(
            Guid templateId, 
            Guid patientId, 
            Guid providerId, 
            Dictionary<Guid, string> fieldValues)
        {
            // Validate inputs
            var template = await GetNoteTemplateByIdAsync(templateId);
            var patient = await _patientRepository.GetByIdAsync(patientId);
            var provider = await _providerRepository.GetByIdAsync(providerId);

            if (patient == null)
                throw new ArgumentException("Patient not found", nameof(patientId));

            if (provider == null)
                throw new ArgumentException("Provider not found", nameof(providerId));

            // Create note template instance
            var instance = new NoteTemplateInstance
            {
                NoteTemplateId = templateId,
                PatientId = patientId,
                ProviderId = providerId,
                NoteTemplate = template
            };

            // Populate and validate fields
            foreach (var section in template.Sections)
            {
                foreach (var field in section.Fields)
                {
                    if (fieldValues.TryGetValue(field.Id, out string fieldValue))
                    {
                        var instanceField = new NoteTemplateInstanceField
                        {
                            NoteTemplateSectionFieldId = field.Id,
                            NoteTemplateSectionField = field,
                            FieldValue = fieldValue
                        };

                        // Validate individual field
                        if (!instanceField.Validate())
                        {
                            _logger.LogWarning($"Invalid field value for {field.FieldName}");
                            throw new ArgumentException($"Invalid field value for {field.FieldName}");
                        }

                        instance.FilledFields.Add(instanceField);
                    }
                    else if (field.IsRequired)
                    {
                        _logger.LogWarning($"Required field {field.FieldName} is missing");
                        throw new ArgumentException($"Required field {field.FieldName} is missing");
                    }
                }
            }

            // Final instance validation
            if (!instance.Validate())
            {
                _logger.LogWarning("Note template instance validation failed");
                throw new ArgumentException("Note template instance validation failed");
            }

            await _instanceRepository.AddAsync(instance);
            return instance;
        }

        public async Task<bool> ValidateNoteTemplateInstanceAsync(NoteTemplateInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            // Ensure template exists
            var template = await _templateRepository.GetByIdAsync(instance.NoteTemplateId);
            if (template == null)
                return false;

            instance.NoteTemplate = template;
            return instance.Validate();
        }

        public async Task<IEnumerable<NoteTemplateInstance>> GetPatientNoteTemplateInstancesAsync(Guid patientId)
        {
            return await _instanceRepository.FindAsync(
                instance => instance.PatientId == patientId
            );
        }
    }
}
