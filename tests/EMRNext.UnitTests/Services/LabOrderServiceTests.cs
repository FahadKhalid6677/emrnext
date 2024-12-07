using System;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Services;
using EMRNext.UnitTests.Helpers;
using Moq;
using Xunit;

namespace EMRNext.UnitTests.Services
{
    public class LabOrderServiceTests
    {
        private readonly Mock<ILabOrderRepository> _mockLabOrderRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly LabOrderService _labOrderService;

        public LabOrderServiceTests()
        {
            _mockLabOrderRepository = new Mock<ILabOrderRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _labOrderService = new LabOrderService(
                _mockLabOrderRepository.Object, 
                _mockNotificationService.Object
            );
        }

        [Fact]
        public async Task CreateLabOrder_ShouldSuccessfullyCreateOrder()
        {
            // Arrange
            var patients = MockDataGenerator.GeneratePatients(1);
            var labOrder = MockDataGenerator.GenerateLabOrders(patients).First();

            _mockLabOrderRepository
                .Setup(repo => repo.AddAsync(It.IsAny<LabOrder>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(service => service.SendNotificationAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<NotificationType>()
                ))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _labOrderService.CreateLabOrderAsync(labOrder);

            // Assert
            Assert.NotNull(result);
            _mockLabOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<LabOrder>()), Times.Once);
            _mockNotificationService.Verify(
                service => service.SendNotificationAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    NotificationType.LabOrder
                ), 
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateLabOrderStatus_ShouldUpdateStatus()
        {
            // Arrange
            var patients = MockDataGenerator.GeneratePatients(1);
            var labOrder = MockDataGenerator.GenerateLabOrders(patients).First();
            labOrder.Status = "Pending";

            _mockLabOrderRepository
                .Setup(repo => repo.GetByIdAsync(labOrder.Id))
                .ReturnsAsync(labOrder);

            _mockLabOrderRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<LabOrder>()))
                .Returns(Task.CompletedTask);

            // Act
            await _labOrderService.UpdateLabOrderStatusAsync(labOrder.Id, "Completed");

            // Assert
            _mockLabOrderRepository.Verify(repo => repo.UpdateAsync(It.Is<LabOrder>(
                lo => lo.Status == "Completed"
            )), Times.Once);
        }

        [Fact]
        public async Task CancelLabOrder_ShouldCancelOrder()
        {
            // Arrange
            var patients = MockDataGenerator.GeneratePatients(1);
            var labOrder = MockDataGenerator.GenerateLabOrders(patients).First();
            labOrder.Status = "Pending";

            _mockLabOrderRepository
                .Setup(repo => repo.GetByIdAsync(labOrder.Id))
                .ReturnsAsync(labOrder);

            _mockLabOrderRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<LabOrder>()))
                .Returns(Task.CompletedTask);

            _mockNotificationService
                .Setup(service => service.SendNotificationAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<NotificationType>()
                ))
                .Returns(Task.CompletedTask);

            // Act
            await _labOrderService.CancelLabOrderAsync(labOrder.Id);

            // Assert
            _mockLabOrderRepository.Verify(repo => repo.UpdateAsync(It.Is<LabOrder>(
                lo => lo.Status == "Cancelled"
            )), Times.Once);

            _mockNotificationService.Verify(
                service => service.SendNotificationAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    NotificationType.LabOrderCancelled
                ), 
                Times.Once
            );
        }
    }
}
