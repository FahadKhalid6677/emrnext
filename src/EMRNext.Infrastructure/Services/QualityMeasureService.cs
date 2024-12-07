using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMRNext.Infrastructure.Services
{
    public class QualityMeasureService : IQualityMeasureService
    {
        private readonly EMRNextDbContext _context;

        public QualityMeasureService(EMRNextDbContext context)
        {
            _context = context;
        }

        public async Task<QualityMeasure> GetMeasureAsync(string measureId)
        {
            return await _context.QualityMeasures
                .Include(m => m.Criteria)
                .FirstOrDefaultAsync(m => m.Id == measureId);
        }

        public async Task<List<QualityMeasure>> GetActiveMeasuresAsync()
        {
            return await _context.QualityMeasures
                .Include(m => m.Criteria)
                .Where(m => m.IsActive)
                .ToListAsync();
        }

        public async Task<MeasureResult> EvaluateMeasureAsync(string measureId, string patientId)
        {
            var measure = await GetMeasureAsync(measureId);
            if (measure == null)
                throw new ArgumentException("Measure not found");

            var result = new MeasureResult
            {
                MeasureId = measureId,
                PatientId = patientId,
                EvaluationDate = DateTime.UtcNow
            };

            try
            {
                // Evaluate each criterion
                foreach (var criterion in measure.Criteria)
                {
                    var criterionMet = await EvaluateCriterionAsync(criterion, patientId);
                    result.CriteriaResults.Add(new CriterionResult
                    {
                        CriterionId = criterion.Id,
                        IsMet = criterionMet
                    });
                }

                // Calculate overall measure status
                result.IsMet = result.CriteriaResults.All(r => r.IsMet);

                // Save result
                _context.MeasureResults.Add(result);
                await _context.SaveChangesAsync();

                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                return result;
            }
        }

        private async Task<bool> EvaluateCriterionAsync(MeasureCriterion criterion, string patientId)
        {
            switch (criterion.Type)
            {
                case "Diagnosis":
                    return await EvaluateDiagnosisCriterionAsync(criterion, patientId);
                case "Medication":
                    return await EvaluateMedicationCriterionAsync(criterion, patientId);
                case "Procedure":
                    return await EvaluateProcedureCriterionAsync(criterion, patientId);
                case "Lab":
                    return await EvaluateLabCriterionAsync(criterion, patientId);
                default:
                    throw new ArgumentException($"Unsupported criterion type: {criterion.Type}");
            }
        }

        private async Task<bool> EvaluateDiagnosisCriterionAsync(MeasureCriterion criterion, string patientId)
        {
            return await _context.Diagnoses
                .AnyAsync(d => d.PatientId == patientId &&
                              d.Code == criterion.Code &&
                              d.Date >= criterion.StartDate &&
                              d.Date <= criterion.EndDate);
        }

        private async Task<bool> EvaluateMedicationCriterionAsync(MeasureCriterion criterion, string patientId)
        {
            return await _context.Medications
                .AnyAsync(m => m.PatientId == patientId &&
                              m.Code == criterion.Code &&
                              m.PrescribedDate >= criterion.StartDate &&
                              m.PrescribedDate <= criterion.EndDate);
        }

        private async Task<bool> EvaluateProcedureCriterionAsync(MeasureCriterion criterion, string patientId)
        {
            return await _context.Procedures
                .AnyAsync(p => p.PatientId == patientId &&
                              p.Code == criterion.Code &&
                              p.Date >= criterion.StartDate &&
                              p.Date <= criterion.EndDate);
        }

        private async Task<bool> EvaluateLabCriterionAsync(MeasureCriterion criterion, string patientId)
        {
            var labResults = await _context.LabResults
                .Where(l => l.PatientId == patientId &&
                           l.TestCode == criterion.Code &&
                           l.Date >= criterion.StartDate &&
                           l.Date <= criterion.EndDate)
                .ToListAsync();

            if (!labResults.Any())
                return false;

            // Evaluate result values against criterion
            foreach (var result in labResults)
            {
                if (EvaluateLabValue(result.Value, criterion.Operator, criterion.Value))
                    return true;
            }

            return false;
        }

        private bool EvaluateLabValue(decimal value, string op, decimal target)
        {
            switch (op)
            {
                case "=":
                    return value == target;
                case ">":
                    return value > target;
                case ">=":
                    return value >= target;
                case "<":
                    return value < target;
                case "<=":
                    return value <= target;
                default:
                    throw new ArgumentException($"Unsupported operator: {op}");
            }
        }
    }

    public class QualityMeasure
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public List<MeasureCriterion> Criteria { get; set; } = new List<MeasureCriterion>();
    }

    public class MeasureCriterion
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Operator { get; set; }
        public decimal Value { get; set; }
    }

    public class MeasureResult
    {
        public string MeasureId { get; set; }
        public string PatientId { get; set; }
        public DateTime EvaluationDate { get; set; }
        public bool IsMet { get; set; }
        public string Error { get; set; }
        public List<CriterionResult> CriteriaResults { get; set; } = new List<CriterionResult>();
    }

    public class CriterionResult
    {
        public string CriterionId { get; set; }
        public bool IsMet { get; set; }
    }
}
