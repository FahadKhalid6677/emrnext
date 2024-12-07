using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.EhrServices
{
    /// <summary>
    /// Service for managing and validating medical note templates
    /// </summary>
    public interface INoteTemplateService
    {
        /// <summary>
        /// Create a new note template
        /// </summary>
        /// <param name="template">Note template to create</param>
        /// <returns>Created note template</returns>
        Task<NoteTemplate> CreateNoteTemplateAsync(NoteTemplate template);

        /// <summary>
        /// Get a note template by its unique identifier
        /// </summary>
        /// <param name="templateId">Unique identifier of the note template</param>
        /// <returns>Note template</returns>
        Task<NoteTemplate> GetNoteTemplateByIdAsync(Guid templateId);

        /// <summary>
        /// Get note templates for a specific medical specialty
        /// </summary>
        /// <param name="medicalSpecialty">Medical specialty</param>
        /// <returns>Collection of note templates</returns>
        Task<IEnumerable<NoteTemplate>> GetNoteTemplatesBySpecialtyAsync(string medicalSpecialty);

        /// <summary>
        /// Create an instance of a note template for a specific patient
        /// </summary>
        /// <param name="templateId">Note template identifier</param>
        /// <param name="patientId">Patient identifier</param>
        /// <param name="providerId">Provider identifier</param>
        /// <param name="fieldValues">Dictionary of field values keyed by field ID</param>
        /// <returns>Created note template instance</returns>
        Task<NoteTemplateInstance> CreateNoteTemplateInstanceAsync(
            Guid templateId, 
            Guid patientId, 
            Guid providerId, 
            Dictionary<Guid, string> fieldValues);

        /// <summary>
        /// Validate a note template instance
        /// </summary>
        /// <param name="instance">Note template instance to validate</param>
        /// <returns>Validation result</returns>
        Task<bool> ValidateNoteTemplateInstanceAsync(NoteTemplateInstance instance);

        /// <summary>
        /// Get all note template instances for a patient
        /// </summary>
        /// <param name="patientId">Patient identifier</param>
        /// <returns>Collection of note template instances</returns>
        Task<IEnumerable<NoteTemplateInstance>> GetPatientNoteTemplateInstancesAsync(Guid patientId);
    }

    /// <summary>
    /// Validation result for note template instances
    /// </summary>
    public class NoteTemplateValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; }
    }
}
