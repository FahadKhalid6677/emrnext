using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMRNext.Infrastructure.Services.External
{
    public interface ILabInterfaceService
    {
        Task<OrderResult> SubmitLabOrderAsync(LabOrder order);
        Task<IEnumerable<LabResult>> GetLabResultsAsync(string orderId);
        Task<IEnumerable<LabTest>> GetAvailableTestsAsync(string facilityId);
        Task<OrderStatus> CheckOrderStatusAsync(string orderId);
        Task<bool> CancelOrderAsync(string orderId, string reason);
        Task<IEnumerable<LabResult>> GetResultsByDateRangeAsync(DateTime startDate, DateTime endDate, string patientId = null);
    }

    public class LabOrder
    {
        public string OrderId { get; set; }
        public string PatientId { get; set; }
        public string ProviderId { get; set; }
        public string FacilityId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Priority { get; set; }
        public List<OrderedTest> Tests { get; set; }
        public string DiagnosisCode { get; set; }
        public string ClinicalNotes { get; set; }
        public string SpecimenType { get; set; }
        public DateTime? CollectionDate { get; set; }
        public string CollectionSite { get; set; }
        public bool FastingStatus { get; set; }
    }

    public class OrderedTest
    {
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Specimen { get; set; }
        public string[] Modifiers { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
    }

    public class OrderResult
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string AccessionNumber { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public string[] Warnings { get; set; }
        public string[] RequiredDocuments { get; set; }
        public IDictionary<string, string> AdditionalInfo { get; set; }
    }

    public class LabResult
    {
        public string ResultId { get; set; }
        public string OrderId { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public DateTime ResultDate { get; set; }
        public string Status { get; set; }
        public List<TestComponent> Components { get; set; }
        public string Interpretation { get; set; }
        public string PerformingLab { get; set; }
        public string TechnologistId { get; set; }
        public string[] Flags { get; set; }
        public string Comments { get; set; }
    }

    public class TestComponent
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Units { get; set; }
        public string ReferenceRange { get; set; }
        public string Flag { get; set; }
        public string Status { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    public class LabTest
    {
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Category { get; set; }
        public string Methodology { get; set; }
        public string SpecimenRequirements { get; set; }
        public string[] SpecimenTypes { get; set; }
        public decimal Cost { get; set; }
        public int TurnaroundTime { get; set; }
        public string[] PrerequisiteTests { get; set; }
        public bool RequiresFasting { get; set; }
        public string[] AvailablePanels { get; set; }
        public IDictionary<string, string> AdditionalRequirements { get; set; }
    }

    public class OrderStatus
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
        public string CurrentStep { get; set; }
        public DateTime? CollectionDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string[] PendingSteps { get; set; }
        public string[] CompletedSteps { get; set; }
        public string[] Issues { get; set; }
        public IDictionary<string, DateTime> Timeline { get; set; }
    }
}
