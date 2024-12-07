using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Services.Clinical;
using EMRNext.Core.Models;
using static EMRNext.Core.Services.Clinical.ClinicalWorkflowService;

namespace EMRNext.Core.Tests.Services.Clinical
{
    public class ClinicalWorkflowServiceTests
    {
        private readonly Mock<ILogger<ClinicalWorkflowService>> _mockLogger;
        private readonly Mock<IQueryable<Patient>> _mockPatientRepository;
        private readonly Mock<IQueryable<ClinicalWorkflow>> _mockWorkflowRepository;

        public ClinicalWorkflowServiceTests()
        {
            _mockLogger = new Mock<ILogger<ClinicalWorkflowService>>();
            _mockPatientRepository = new Mock<IQueryable<Patient>>();
            _mockWorkflowRepository = new Mock<IQueryable<ClinicalWorkflow>>();
        }

        [Fact]
        public async Task CreateWorkflow_ValidPatient_ReturnsWorkflow()
        {
            // Arrange
            var patientId = "TEST_PATIENT_001";
            var patient = new Patient { Id = patientId, Name = "Test Patient" };
            
            _mockPatientRepository
                .Setup(repo => repo.FirstOrDefault(It.IsAny<Func<Patient, bool>>()))
                .Returns(patient);

            var workflowService = new ClinicalWorkflowService(
                _mockLogger.Object, 
                _mockPatientRepository.Object, 
                _mockWorkflowRepository.Object
            );

            // Act
            var workflow = await workflowService.CreateWorkflowAsync(
                patientId, 
                "MedicalConsultation"
            );

            // Assert
            Assert.NotNull(workflow);
            Assert.Equal(patientId, workflow.PatientId);
            Assert.Equal("MedicalConsultation", workflow.WorkflowType);
            Assert.Equal(WorkflowStatus.Pending, workflow.Status);
            Assert.Equal(4, workflow.Steps.Count);
        }

        [Fact]
        public async Task CreateWorkflow_InvalidPatient_ThrowsArgumentException()
        {
            // Arrange
            _mockPatientRepository
                .Setup(repo => repo.FirstOrDefault(It.IsAny<Func<Patient, bool>>()))
                .Returns((Patient)null);

            var workflowService = new ClinicalWorkflowService(
                _mockLogger.Object, 
                _mockPatientRepository.Object, 
                _mockWorkflowRepository.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                workflowService.CreateWorkflowAsync("INVALID_PATIENT", "MedicalConsultation")
            );
        }

        [Fact]
        public async Task UpdateWorkflowStatus_ValidWorkflow_UpdatesStatus()
        {
            // Arrange
            var workflow = new ClinicalWorkflow
            {
                Id = Guid.NewGuid(),
                Status = WorkflowStatus.Pending,
                Steps = new System.Collections.Generic.List<WorkflowStep>()
            };

            _mockWorkflowRepository
                .Setup(repo => repo.FirstOrDefault(It.IsAny<Func<ClinicalWorkflow, bool>>()))
                .Returns(workflow);

            var workflowService = new ClinicalWorkflowService(
                _mockLogger.Object, 
                _mockPatientRepository.Object, 
                _mockWorkflowRepository.Object
            );

            // Act
            await workflowService.UpdateWorkflowStatusAsync(
                workflow.Id, 
                WorkflowStatus.InProgress
            );

            // Assert
            Assert.Equal(WorkflowStatus.InProgress, workflow.Status);
            Assert.NotNull(workflow.UpdatedAt);
        }

        [Fact]
        public async Task CompleteWorkflowStep_AllStepsCompleted_WorkflowCompleted()
        {
            // Arrange
            var workflow = new ClinicalWorkflow
            {
                Id = Guid.NewGuid(),
                Status = WorkflowStatus.Pending,
                Steps = new System.Collections.Generic.List<WorkflowStep>
                {
                    new WorkflowStep 
                    { 
                        Id = Guid.NewGuid(), 
                        StepStatus = WorkflowStatus.Pending 
                    },
                    new WorkflowStep 
                    { 
                        Id = Guid.NewGuid(), 
                        StepStatus = WorkflowStatus.Pending 
                    }
                }
            };

            _mockWorkflowRepository
                .Setup(repo => repo.FirstOrDefault(It.IsAny<Func<ClinicalWorkflow, bool>>()))
                .Returns(workflow);

            var workflowService = new ClinicalWorkflowService(
                _mockLogger.Object, 
                _mockPatientRepository.Object, 
                _mockWorkflowRepository.Object
            );

            var stepId = workflow.Steps[0].Id;
            var action = new WorkflowStepAction
            {
                Id = Guid.NewGuid(),
                ActionName = "Test Action",
                PerformedAt = DateTime.UtcNow
            };

            // Act
            await workflowService.CompleteWorkflowStepAsync(workflow.Id, stepId, action);
            await workflowService.CompleteWorkflowStepAsync(
                workflow.Id, 
                workflow.Steps[1].Id, 
                new WorkflowStepAction
                {
                    Id = Guid.NewGuid(),
                    ActionName = "Test Action 2",
                    PerformedAt = DateTime.UtcNow
                }
            );

            // Assert
            Assert.Equal(WorkflowStatus.Completed, workflow.Status);
            Assert.All(workflow.Steps, step => 
                Assert.Equal(WorkflowStatus.Completed, step.StepStatus)
            );
        }
    }
}
