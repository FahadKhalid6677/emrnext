using System;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Infrastructure.Reporting;
using EMRNext.Core.Models;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Services.Reporting
{
    public class ClinicalReportService : 
        ReportingEngine.IReportDataSource, 
        ReportingEngine.IReportFilter, 
        ReportingEngine.IReportAggregator
    {
        private readonly ILogger<ClinicalReportService> _logger;
        private readonly IQueryable<Patient> _patientRepository;
        private readonly IQueryable<Prescription> _prescriptionRepository;

        public ClinicalReportService(
            ILogger<ClinicalReportService> logger,
            IQueryable<Patient> patientRepository,
            IQueryable<Prescription> prescriptionRepository)
        {
            _logger = logger;
            _patientRepository = patientRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        // Data Source Implementation
        public async Task<IQueryable<object>> GetDataAsync()
        {
            // Combine patient and prescription data
            var combinedData = _patientRepository
                .Join(_prescriptionRepository, 
                    patient => patient.Id, 
                    prescription => prescription.PatientId,
                    (patient, prescription) => new 
                    {
                        Patient = patient,
                        Prescription = prescription
                    })
                .AsQueryable<object>();

            return await Task.FromResult(combinedData);
        }

        // Filter Implementation
        public IQueryable<object> ApplyFilter(IQueryable<object> data)
        {
            // Example: Filter for recent prescriptions
            return data.Where(item => 
                ((dynamic)item).Prescription.PrescriptionDate > DateTime.Now.AddMonths(-3)
            );
        }

        // Aggregation Implementation
        public object Aggregate(IQueryable<object> data)
        {
            // Example: Aggregate prescription statistics
            var aggregatedData = data
                .GroupBy(item => ((dynamic)item).Patient.Age)
                .Select(group => new 
                {
                    AgeGroup = group.Key,
                    TotalPrescriptions = group.Count(),
                    UniquePatients = group.Select(x => ((dynamic)x).Patient.Id).Distinct().Count(),
                    AveragePrescriptionCost = group.Average(x => ((dynamic)x).Prescription.Cost)
                });

            return aggregatedData;
        }
    }
}
