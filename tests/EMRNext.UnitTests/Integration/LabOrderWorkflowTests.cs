using System;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Entities;
using EMRNext.Infrastructure.Data;
using EMRNext.Infrastructure.Repositories;
using EMRNext.Infrastructure.Services;
using EMRNext.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EMRNext.UnitTests.Integration
{
    public class LabOrderWorkflowTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PatientRepository _patientRepository;
        private readonly LabOrderRepository _labOrderRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly LabOrderService _labOrderService;

        public LabOrderWorkflowTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _patientRepository = new PatientRepository(_context);
            _labOrderRepository = new LabOrderRepository(_context);
            
            _mockNotificationService = new Mock<INotificationService>();
            _mockNotificationService
                .Setup(service => service.SendNotificationAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<NotificationType>()
                ))
                .Returns(Task.CompletedTask);

            _labOrderService = new LabOrderService(
                _labOrderRepository, 
                _mockNotificationService.Object
            );
        }

        [Fact]
        public async Task CompleteLabOrderWorkflow_ShouldSucceed()
        {
            // Arrange: Create Patient
            var patient = MockDataGenerator.GeneratePatients(1).First();
            await _patientRepository.AddAsync(patient);
            await _context.SaveChangesAsync();

            // Act: Create Lab Order
            var labOrder = new LabOrder
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TestType = "Blood Test"
            };

            await _labOrderService.CreateLabOrderAsync(labOrder);

            // Assert: Verify Lab Order Creation
            var createdLabOrder = await _labOrderRepository.GetByIdAsync(labOrder.Id);
            Assert.NotNull(createdLabOrder);
            Assert.Equal("Pending", createdLabOrder.Status);

            // Act: Update Lab Order Status
            await _labOrderService.UpdateLabOrderStatusAsync(labOrder.Id, "In Progress");

            // Assert: Verify Status Update
            var updatedLabOrder = await _labOrderRepository.GetByIdAsync(labOrder.Id);
            Assert.Equal("In Progress", updatedLabOrder.Status);

            // Act: Complete Lab Order
            await _labOrderService.UpdateLabOrderStatusAsync(labOrder.Id, "Completed");

            // Assert: Verify Completion
            var completedLabOrder = await _labOrderRepository.GetByIdAsync(labOrder.Id);
            Assert.Equal("Completed", completedLabOrder.Status);

            // Verify Notification Calls
            _mockNotificationService.Verify(
                service => service.SendNotificationAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<NotificationType>()
                ), 
                Times.Exactly(3)  // Creation, In Progress, Completed
            );
        }

        [Fact]
        public async Task CancelLabOrder_ShouldSucceed()
        {
            // Arrange: Create Patient
            var patient = MockDataGenerator.GeneratePatients(1).First();
            await _patientRepository.AddAsync(patient);
            await _context.SaveChangesAsync();

            // Act: Create Lab Order
            var labOrder = new LabOrder
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TestType = "X-Ray"
            };

            await _labOrderService.CreateLabOrderAsync(labOrder);

            // Act: Cancel Lab Order
            await _labOrderService.CancelLabOrderAsync(labOrder.Id);

            // Assert: Verify Cancellation
            var cancelledLabOrder = await _labOrderRepository.GetByIdAsync(labOrder.Id);
            Assert.Equal("Cancelled", cancelledLabOrder.Status);

            // Verify Notification Calls
            _mockNotificationService.Verify(
                service => service.SendNotificationAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    NotificationType.LabOrderCancelled
                ), 
                Times.Once
            );
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
