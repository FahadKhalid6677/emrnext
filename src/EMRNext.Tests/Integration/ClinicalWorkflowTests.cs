using System;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EMRNext.Tests.Integration
{
    public class ClinicalWorkflowTests : IClassFixture<TestFixture>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthenticationService _authService;
        private readonly IPatientService _patientService;
        private readonly IDocumentationService _documentService;
        private readonly IOrderService _orderService;

        public ClinicalWorkflowTests(TestFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;
            _authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
            _patientService = _serviceProvider.GetRequiredService<IPatientService>();
            _documentService = _serviceProvider.GetRequiredService<IDocumentationService>();
            _orderService = _serviceProvider.GetRequiredService<IOrderService>();
        }

        [Fact]
        public async Task CompleteWorkflow_Success()
        {
            // 1. Authentication
            var authResult = await _authService.LoginAsync("john.doe@test.com", "test123");
            Assert.True(authResult.IsSuccess);
            Assert.NotNull(authResult.Token);

            // 2. Patient Context
            var patient = await _patientService.GetByMRNAsync("MRN001");
            Assert.NotNull(patient);
            Assert.Equal("Alice", patient.FirstName);
            Assert.Equal("Johnson", patient.LastName);

            // 3. Problem List
            var problems = await _patientService.GetProblemsAsync(patient.Id);
            Assert.Contains(problems, p => p.Code == "I10"); // Hypertension
            Assert.Contains(problems, p => p.Code == "E11.9"); // Diabetes

            // 4. Create Documentation
            var note = await _documentService.CreateNoteAsync(new ClinicalNote
            {
                PatientId = patient.Id,
                Type = "Progress Note",
                Status = "Draft"
            });
            Assert.NotNull(note.Id);

            // 5. Add Vital Signs
            await _documentService.AddVitalSignAsync(new VitalSign
            {
                NoteId = note.Id,
                Type = "blood_pressure",
                Value = "120/80",
                Unit = "mmHg",
                Timestamp = DateTime.UtcNow
            });

            // 6. Create Order
            var order = await _orderService.CreateOrderAsync(new Order
            {
                PatientId = patient.Id,
                NoteId = note.Id,
                Type = "lab",
                Name = "Basic Metabolic Panel",
                Priority = "routine"
            });
            Assert.NotNull(order.Id);

            // 7. Apply Template
            await _documentService.ApplyTemplateAsync(note.Id, "Normal Physical Exam");
            var updatedNote = await _documentService.GetNoteAsync(note.Id);
            Assert.Contains("Constitutional: Alert and oriented", updatedNote.Content);

            // 8. Sign Document
            var signResult = await _documentService.SignNoteAsync(note.Id);
            Assert.True(signResult.IsSuccess);
            
            // 9. Verify Final State
            var finalNote = await _documentService.GetNoteAsync(note.Id);
            Assert.Equal("Signed", finalNote.Status);
            Assert.NotNull(finalNote.SignedAt);
            Assert.NotNull(finalNote.SignedBy);
        }

        [Fact]
        public async Task ErrorScenarios_HandledCorrectly()
        {
            // 1. Invalid Login
            var badAuthResult = await _authService.LoginAsync("invalid@test.com", "wrongpass");
            Assert.False(badAuthResult.IsSuccess);

            // 2. Missing Required Fields
            await Assert.ThrowsAsync<ValidationException>(() => 
                _documentService.CreateNoteAsync(new ClinicalNote()));

            // 3. Invalid Patient
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _patientService.GetByMRNAsync("INVALID_MRN"));

            // 4. Unsigned Note Validation
            var note = await _documentService.CreateNoteAsync(new ClinicalNote
            {
                PatientId = "1",
                Type = "Progress Note",
                Status = "Draft"
            });
            
            // Attempt to finalize without signature
            await Assert.ThrowsAsync<ValidationException>(() =>
                _documentService.FinalizeNoteAsync(note.Id));
        }
    }
}
