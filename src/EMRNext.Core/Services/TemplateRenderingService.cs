using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using EMRNext.Core.Domain.Entities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EMRNext.Core.Services
{
    public class TemplateRenderingService
    {
        private readonly EMRDbContext _context;
        private readonly IClinicalDecisionSupportService _clinicalService;
        private readonly IVariableResolutionService _variableService;

        public TemplateRenderingService(
            EMRDbContext context,
            IClinicalDecisionSupportService clinicalService,
            IVariableResolutionService variableService)
        {
            _context = context;
            _clinicalService = clinicalService;
            _variableService = variableService;
        }

        public async Task<string> RenderTemplateAsync(ClinicalTemplate template, Dictionary<string, object> context)
        {
            var renderedContent = new StringBuilder();
            var clinicalContext = await BuildClinicalContextAsync(context);

            // Add template header
            await RenderTemplateHeaderAsync(template, renderedContent, clinicalContext);

            // Render sections
            foreach (var section in template.Sections.OrderBy(s => s.OrderIndex))
            {
                if (await ShouldRenderSectionAsync(section, clinicalContext))
                {
                    await RenderSectionAsync(section, renderedContent, clinicalContext);
                }
            }

            // Add template footer
            await RenderTemplateFooterAsync(template, renderedContent, clinicalContext);

            return renderedContent.ToString();
        }

        private async Task<Dictionary<string, object>> BuildClinicalContextAsync(Dictionary<string, object> context)
        {
            var clinicalContext = new Dictionary<string, object>(context);

            // Add encounter data if available
            if (context.ContainsKey("EncounterId"))
            {
                var encounterId = Convert.ToInt32(context["EncounterId"]);
                var encounter = await _context.Encounters
                    .Include(e => e.Patient)
                    .Include(e => e.Provider)
                    .FirstOrDefaultAsync(e => e.Id == encounterId);

                if (encounter != null)
                {
                    clinicalContext["Encounter"] = encounter;
                    clinicalContext["Patient"] = encounter.Patient;
                    clinicalContext["Provider"] = encounter.Provider;
                }
            }

            // Add clinical data
            if (clinicalContext.ContainsKey("Patient"))
            {
                var patient = (Patient)clinicalContext["Patient"];
                var vitals = await _context.Vitals
                    .Where(v => v.PatientId == patient.Id)
                    .OrderByDescending(v => v.RecordedAt)
                    .FirstOrDefaultAsync();
                clinicalContext["CurrentVitals"] = vitals;

                var medications = await _context.Medications
                    .Where(m => m.PatientId == patient.Id && m.IsActive)
                    .ToListAsync();
                clinicalContext["ActiveMedications"] = medications;

                var problems = await _context.Problems
                    .Where(p => p.PatientId == patient.Id && p.IsActive)
                    .ToListAsync();
                clinicalContext["ActiveProblems"] = problems;
            }

            return clinicalContext;
        }

        private async Task RenderTemplateHeaderAsync(ClinicalTemplate template, StringBuilder output, Dictionary<string, object> context)
        {
            output.AppendLine($"<h1>{template.Name}</h1>");
            
            if (!string.IsNullOrEmpty(template.HeaderContent))
            {
                var renderedHeader = await ResolveVariablesAsync(template.HeaderContent, context);
                output.AppendLine(renderedHeader);
            }

            // Add metadata
            if (context.ContainsKey("Patient"))
            {
                var patient = (Patient)context["Patient"];
                output.AppendLine($"<div class='patient-info'>");
                output.AppendLine($"<p>Patient: {patient.LastName}, {patient.FirstName}</p>");
                output.AppendLine($"<p>DOB: {patient.DateOfBirth:MM/dd/yyyy}</p>");
                output.AppendLine($"<p>MRN: {patient.MedicalRecordNumber}</p>");
                output.AppendLine("</div>");
            }

            if (context.ContainsKey("Encounter"))
            {
                var encounter = (Encounter)context["Encounter"];
                output.AppendLine($"<div class='encounter-info'>");
                output.AppendLine($"<p>Date: {encounter.EncounterDate:MM/dd/yyyy}</p>");
                output.AppendLine($"<p>Provider: {encounter.Provider.LastName}, {encounter.Provider.FirstName}</p>");
                output.AppendLine($"<p>Type: {encounter.EncounterType}</p>");
                output.AppendLine("</div>");
            }
        }

        private async Task<bool> ShouldRenderSectionAsync(TemplateSection section, Dictionary<string, object> context)
        {
            if (!section.HasConditions)
                return true;

            var conditions = JsonConvert.DeserializeObject<Dictionary<string, object>>(section.DisplayConditions);
            return await EvaluateConditionsAsync(conditions, context);
        }

        private async Task RenderSectionAsync(TemplateSection section, StringBuilder output, Dictionary<string, object> context)
        {
            output.AppendLine($"<div class='section' id='section-{section.Id}'>");
            output.AppendLine($"<h2>{section.Name}</h2>");

            if (!string.IsNullOrEmpty(section.Description))
                output.AppendLine($"<p class='section-description'>{section.Description}</p>");

            // Render section content with resolved variables
            if (!string.IsNullOrEmpty(section.Content))
            {
                var renderedContent = await ResolveVariablesAsync(section.Content, context);
                output.AppendLine(renderedContent);
            }

            // Render fields
            if (section.Fields != null && section.Fields.Any())
            {
                await RenderFieldsAsync(section.Fields, output, context);
            }

            // Add clinical decision support if enabled
            if (section.HasConditions && context.ContainsKey("EncounterId"))
            {
                var encounterId = Convert.ToInt32(context["EncounterId"]);
                var alerts = await _clinicalService.GetAlertsForEncounterAsync(encounterId);
                if (alerts.Any())
                {
                    output.AppendLine("<div class='clinical-alerts'>");
                    foreach (var alert in alerts)
                    {
                        output.AppendLine($"<div class='alert alert-{alert.Severity.ToLower()}'>{alert.Message}</div>");
                    }
                    output.AppendLine("</div>");
                }
            }

            output.AppendLine("</div>");
        }

        private async Task RenderFieldsAsync(IEnumerable<TemplateField> fields, StringBuilder output, Dictionary<string, object> context)
        {
            output.AppendLine("<div class='fields'>");
            
            foreach (var field in fields.OrderBy(f => f.OrderIndex))
            {
                if (!await ShouldRenderFieldAsync(field, context))
                    continue;

                output.AppendLine($"<div class='field' id='field-{field.Id}'>");
                
                // Render label
                output.AppendLine($"<label for='{field.Name}'>{field.Label}</label>");

                // Render input based on field type
                await RenderFieldInputAsync(field, output, context);

                // Add validation messages if any
                if (field.EnableValidation)
                {
                    output.AppendLine($"<span class='validation-message' data-rules='{field.ValidationRules}'>{field.ValidationMessage}</span>");
                }

                output.AppendLine("</div>");
            }

            output.AppendLine("</div>");
        }

        private async Task<bool> ShouldRenderFieldAsync(TemplateField field, Dictionary<string, object> context)
        {
            if (string.IsNullOrEmpty(field.DisplayCondition))
                return true;

            try
            {
                var condition = JsonConvert.DeserializeObject<Dictionary<string, object>>(field.DisplayCondition);
                return await EvaluateConditionsAsync(condition, context);
            }
            catch
            {
                return true;
            }
        }

        private async Task RenderFieldInputAsync(TemplateField field, StringBuilder output, Dictionary<string, object> context)
        {
            var value = await GetFieldValueAsync(field, context);
            var attributes = GetFieldAttributes(field);

            switch (field.FieldType.ToLower())
            {
                case "text":
                    output.AppendLine($"<input type='text' name='{field.Name}' value='{value}' {attributes} />");
                    break;
                case "textarea":
                    output.AppendLine($"<textarea name='{field.Name}' {attributes}>{value}</textarea>");
                    break;
                case "select":
                    await RenderSelectFieldAsync(field, output, value, attributes);
                    break;
                case "radio":
                    await RenderRadioFieldAsync(field, output, value, attributes);
                    break;
                case "checkbox":
                    output.AppendLine($"<input type='checkbox' name='{field.Name}' value='true' {(value == "true" ? "checked" : "")} {attributes} />");
                    break;
                case "date":
                    output.AppendLine($"<input type='date' name='{field.Name}' value='{value}' {attributes} />");
                    break;
                case "number":
                    output.AppendLine($"<input type='number' name='{field.Name}' value='{value}' {attributes} />");
                    break;
                default:
                    output.AppendLine($"<input type='text' name='{field.Name}' value='{value}' {attributes} />");
                    break;
            }
        }

        private async Task<string> ResolveVariablesAsync(string content, Dictionary<string, object> context)
        {
            var pattern = @"\{\{(.*?)\}\}";
            return await Regex.Replace(content, pattern, async match =>
            {
                var variableName = match.Groups[1].Value.Trim();
                return await _variableService.ResolveVariableAsync(variableName, context);
            });
        }

        private async Task<bool> EvaluateConditionsAsync(Dictionary<string, object> conditions, Dictionary<string, object> context)
        {
            // Implement condition evaluation logic
            return true; // Placeholder
        }

        private string GetFieldAttributes(TemplateField field)
        {
            var attributes = new List<string>();

            if (field.IsRequired)
                attributes.Add("required");
            if (field.IsReadOnly)
                attributes.Add("readonly");
            if (!string.IsNullOrEmpty(field.Placeholder))
                attributes.Add($"placeholder='{field.Placeholder}'");
            if (!string.IsNullOrEmpty(field.CssClass))
                attributes.Add($"class='{field.CssClass}'");
            if (field.MinLength.HasValue)
                attributes.Add($"minlength='{field.MinLength}'");
            if (field.MaxLength.HasValue)
                attributes.Add($"maxlength='{field.MaxLength}'");
            if (field.MinValue.HasValue)
                attributes.Add($"min='{field.MinValue}'");
            if (field.MaxValue.HasValue)
                attributes.Add($"max='{field.MaxValue}'");
            if (!string.IsNullOrEmpty(field.RegexPattern))
                attributes.Add($"pattern='{field.RegexPattern}'");

            return string.Join(" ", attributes);
        }

        private async Task<string> GetFieldValueAsync(TemplateField field, Dictionary<string, object> context)
        {
            if (field.IsCalculated)
            {
                return await CalculateFieldValueAsync(field, context);
            }

            if (context.ContainsKey(field.Name))
            {
                return context[field.Name]?.ToString() ?? string.Empty;
            }

            return field.DefaultValue ?? string.Empty;
        }

        private async Task<string> CalculateFieldValueAsync(TemplateField field, Dictionary<string, object> context)
        {
            try
            {
                // Implement calculation logic based on field.CalculationFormula
                return string.Empty; // Placeholder
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task RenderTemplateFooterAsync(ClinicalTemplate template, StringBuilder output, Dictionary<string, object> context)
        {
            if (!string.IsNullOrEmpty(template.FooterContent))
            {
                var renderedFooter = await ResolveVariablesAsync(template.FooterContent, context);
                output.AppendLine(renderedFooter);
            }

            // Add signature blocks if required
            if (template.RequiresSignature)
            {
                output.AppendLine("<div class='signature-block'>");
                output.AppendLine("<p>Provider Signature: _________________________</p>");
                output.AppendLine("<p>Date: _________________________</p>");
                
                if (template.RequiresCoSign)
                {
                    output.AppendLine("<p>Co-Signer: _________________________</p>");
                    output.AppendLine("<p>Date: _________________________</p>");
                }
                
                output.AppendLine("</div>");
            }
        }
    }
}
