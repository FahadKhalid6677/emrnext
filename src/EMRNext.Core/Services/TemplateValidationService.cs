using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EMRNext.Core.Services
{
    public class TemplateValidationService
    {
        private readonly EMRDbContext _context;
        private readonly IClinicalDecisionSupportService _clinicalService;

        public TemplateValidationService(
            EMRDbContext context,
            IClinicalDecisionSupportService clinicalService)
        {
            _context = context;
            _clinicalService = clinicalService;
        }

        public async Task<Dictionary<string, List<string>>> ValidateTemplateAsync(ClinicalTemplate template)
        {
            var validationResults = new Dictionary<string, List<string>>();

            await ValidateTemplateStructure(template, validationResults);
            await ValidateTemplateContent(template, validationResults);
            await ValidateClinicalCompliance(template, validationResults);
            await ValidateDataBindings(template, validationResults);
            await ValidateSecurityRequirements(template, validationResults);

            return validationResults;
        }

        private async Task ValidateTemplateStructure(ClinicalTemplate template, Dictionary<string, List<string>> results)
        {
            var errors = new List<string>();

            // Basic template validation
            if (string.IsNullOrEmpty(template.Name))
                errors.Add("Template name is required");
            if (string.IsNullOrEmpty(template.Category))
                errors.Add("Template category is required");
            if (string.IsNullOrEmpty(template.SpecialtyType))
                errors.Add("Specialty type is required");

            // Section validation
            if (template.Sections == null || !template.Sections.Any())
                errors.Add("Template must contain at least one section");
            else
            {
                var sectionErrors = await ValidateSections(template.Sections);
                if (sectionErrors.Any())
                    errors.AddRange(sectionErrors);
            }

            if (errors.Any())
                results.Add("Structure", errors);
        }

        private async Task ValidateTemplateContent(ClinicalTemplate template, Dictionary<string, List<string>> results)
        {
            var errors = new List<string>();

            // Variable validation
            var variables = await ExtractTemplateVariables(template);
            foreach (var variable in variables)
            {
                if (!await IsVariableValid(variable))
                    errors.Add($"Invalid variable reference: {variable}");
            }

            // Content formatting
            foreach (var section in template.Sections)
            {
                if (!IsValidContentFormat(section.Content))
                    errors.Add($"Invalid content format in section: {section.Name}");

                foreach (var field in section.Fields)
                {
                    if (!IsValidFieldConfiguration(field))
                        errors.Add($"Invalid field configuration: {field.Name}");
                }
            }

            if (errors.Any())
                results.Add("Content", errors);
        }

        private async Task ValidateClinicalCompliance(ClinicalTemplate template, Dictionary<string, List<string>> results)
        {
            var errors = new List<string>();

            // Required clinical fields
            if (template.Purpose == "Progress Note" || template.Purpose == "History and Physical")
            {
                var requiredFields = await GetRequiredClinicalFields(template.Purpose);
                foreach (var field in requiredFields)
                {
                    if (!HasRequiredField(template, field))
                        errors.Add($"Missing required clinical field: {field}");
                }
            }

            // Clinical decision support rules
            if (template.EnableDecisionSupport)
            {
                var ruleIds = JsonConvert.DeserializeObject<List<int>>(template.AssociatedRules);
                foreach (var ruleId in ruleIds)
                {
                    if (!await IsValidClinicalRule(ruleId))
                        errors.Add($"Invalid clinical rule reference: {ruleId}");
                }
            }

            // Terminology validation
            foreach (var section in template.Sections)
            {
                foreach (var field in section.Fields)
                {
                    if (!string.IsNullOrEmpty(field.SnowmedCode))
                    {
                        if (!await IsValidSnowmedCode(field.SnowmedCode))
                            errors.Add($"Invalid SNOMED code: {field.SnowmedCode}");
                    }
                    if (!string.IsNullOrEmpty(field.LoincCode))
                    {
                        if (!await IsValidLoincCode(field.LoincCode))
                            errors.Add($"Invalid LOINC code: {field.LoincCode}");
                    }
                }
            }

            if (errors.Any())
                results.Add("Clinical", errors);
        }

        private async Task ValidateDataBindings(ClinicalTemplate template, Dictionary<string, List<string>> results)
        {
            var errors = new List<string>();

            foreach (var section in template.Sections)
            {
                if (!string.IsNullOrEmpty(section.DataSource))
                {
                    var dataSourceConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(section.DataSource);
                    if (!await IsValidDataSource(dataSourceConfig))
                        errors.Add($"Invalid data source configuration in section: {section.Name}");
                }

                foreach (var field in section.Fields)
                {
                    if (field.IsCalculated)
                    {
                        if (!IsValidCalculationFormula(field.CalculationFormula))
                            errors.Add($"Invalid calculation formula in field: {field.Name}");
                    }
                }
            }

            if (errors.Any())
                results.Add("DataBinding", errors);
        }

        private async Task ValidateSecurityRequirements(ClinicalTemplate template, Dictionary<string, List<string>> results)
        {
            var errors = new List<string>();

            // Access control validation
            if (string.IsNullOrEmpty(template.AllowedRoles))
                errors.Add("Template must specify allowed roles");

            // PHI field validation
            foreach (var section in template.Sections)
            {
                foreach (var field in section.Fields)
                {
                    if (IsPHIField(field) && !field.IsEncrypted)
                        errors.Add($"PHI field must be encrypted: {field.Name}");
                }
            }

            if (errors.Any())
                results.Add("Security", errors);
        }

        // Helper methods
        private async Task<List<string>> ValidateSections(ICollection<TemplateSection> sections)
        {
            var errors = new List<string>();
            var orderIndices = sections.Select(s => s.OrderIndex).ToList();

            if (orderIndices.Distinct().Count() != sections.Count)
                errors.Add("Duplicate section order indices found");

            if (orderIndices.Any(i => i < 0))
                errors.Add("Section order indices must be non-negative");

            return errors;
        }

        private async Task<List<string>> ExtractTemplateVariables(ClinicalTemplate template)
        {
            var variables = new List<string>();
            var pattern = @"\{\{(.*?)\}\}";

            foreach (var section in template.Sections)
            {
                var matches = Regex.Matches(section.Content, pattern);
                variables.AddRange(matches.Select(m => m.Groups[1].Value.Trim()));
            }

            return variables.Distinct().ToList();
        }

        private bool IsValidContentFormat(string content)
        {
            if (string.IsNullOrEmpty(content))
                return true;

            try
            {
                // Check for well-formed HTML/markup
                return true; // Implement actual validation logic
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidFieldConfiguration(TemplateField field)
        {
            if (string.IsNullOrEmpty(field.FieldType))
                return false;

            if (field.IsRequired && string.IsNullOrEmpty(field.ValidationMessage))
                return false;

            return true;
        }

        private async Task<List<string>> GetRequiredClinicalFields(string purpose)
        {
            // Return list of required fields based on template purpose
            return new List<string>(); // Implement actual logic
        }

        private bool HasRequiredField(ClinicalTemplate template, string fieldName)
        {
            return template.Sections
                .SelectMany(s => s.Fields)
                .Any(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> IsValidClinicalRule(int ruleId)
        {
            var rule = await _clinicalService.GetRuleByIdAsync(ruleId);
            return rule != null;
        }

        private async Task<bool> IsValidSnowmedCode(string code)
        {
            // Validate SNOMED CT code
            return true; // Implement actual validation
        }

        private async Task<bool> IsValidLoincCode(string code)
        {
            // Validate LOINC code
            return true; // Implement actual validation
        }

        private async Task<bool> IsValidDataSource(Dictionary<string, object> config)
        {
            // Validate data source configuration
            return true; // Implement actual validation
        }

        private bool IsValidCalculationFormula(string formula)
        {
            if (string.IsNullOrEmpty(formula))
                return false;

            try
            {
                // Validate calculation formula syntax
                return true; // Implement actual validation
            }
            catch
            {
                return false;
            }
        }

        private bool IsPHIField(TemplateField field)
        {
            // Define PHI field types
            var phiTypes = new[] { "SSN", "MRN", "DOB", "Address", "Phone" };
            return phiTypes.Contains(field.FieldType);
        }
    }
}
