using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Services;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EMRNext.Core.Tests.Services
{
    public class VitalServiceTests
    {
        private readonly Mock<EMRNextDbContext> _mockContext;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly VitalService _vitalService;
        private readonly Mock<DbSet<Vital>> _mockVitalDbSet;
        private readonly Mock<DbSet<Patient>> _mockPatientDbSet;

        public VitalServiceTests()
        {
            _mockContext = new Mock<EMRNextDbContext>();
            _mockAuditService = new Mock<IAuditService>();
            _mockVitalDbSet = new Mock<DbSet<Vital>>();
            _mockPatientDbSet = new Mock<DbSet<Patient>>();

            _mockContext.Setup(c => c.Vitals).Returns(_mockVitalDbSet.Object);
            _mockContext.Setup(c => c.Patients).Returns(_mockPatientDbSet.Object);

            _vitalService = new VitalService(_mockContext.Object, _mockAuditService.Object);
        }

        [Fact]
        public async Task GetVitalByIdAsync_ExistingId_ReturnsVital()
        {
            // Arrange
            var vitalId = 1;
            var expectedVital = new Vital { Id = vitalId, PatientId = 1 };

            _mockVitalDbSet.Setup(d => d.FindAsync(vitalId))
                .ReturnsAsync(expectedVital);

            // Act
            var result = await _vitalService.GetVitalByIdAsync(vitalId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(vitalId, result.Id);
        }

        [Fact]
        public async Task AddVitalAsync_ValidVital_ReturnsAddedVital()
        {
            // Arrange
            var vital = new Vital
            {
                PatientId = 1,
                EncounterId = 1,
                Temperature = 98.6M,
                TemperatureUnit = "F",
                Pulse = 72M,
                RespiratoryRate = 16M
            };

            _mockVitalDbSet.Setup(d => d.Add(It.IsAny<Vital>()))
                .Callback<Vital>(v => { v.Id = 1; });

            // Act
            var result = await _vitalService.AddVitalAsync(vital);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
            _mockAuditService.Verify(a => a.LogActivityAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateVitalAsync_ExistingVital_ReturnsUpdatedVital()
        {
            // Arrange
            var vitalId = 1;
            var existingVital = new Vital { Id = vitalId, PatientId = 1, Temperature = 98.6M };
            var updatedVital = new Vital { Id = vitalId, PatientId = 1, Temperature = 99.0M };

            _mockVitalDbSet.Setup(d => d.FindAsync(vitalId))
                .ReturnsAsync(existingVital);

            // Act
            var result = await _vitalService.UpdateVitalAsync(updatedVital);

            // Assert
            Assert.NotNull(result);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
            _mockAuditService.Verify(a => a.LogActivityAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteVitalAsync_ExistingVital_ReturnsTrue()
        {
            // Arrange
            var vitalId = 1;
            var vital = new Vital { Id = vitalId, PatientId = 1 };

            _mockVitalDbSet.Setup(d => d.FindAsync(vitalId))
                .ReturnsAsync(vital);

            // Act
            var result = await _vitalService.DeleteVitalAsync(vitalId);

            // Assert
            Assert.True(result);
            _mockVitalDbSet.Verify(d => d.Remove(vital), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
            _mockAuditService.Verify(a => a.LogActivityAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CalculateBMIAsync_ValidInputs_ReturnsCorrectBMI()
        {
            // Arrange
            decimal height = 70; // inches
            decimal weight = 150; // pounds

            // Act
            var bmi = await _vitalService.CalculateBMIAsync(height, weight);

            // Assert
            Assert.NotNull(bmi);
            Assert.Equal(21.5M, bmi.Value, 1); // BMI should be ~21.5
        }

        [Theory]
        [InlineData(98.6, true)]  // Normal temperature
        [InlineData(103.0, false)] // High temperature
        [InlineData(95.0, false)]  // Low temperature
        public async Task ValidateVitalRangesAsync_Temperature_ReturnsExpectedResult(decimal temperature, bool expectedResult)
        {
            // Arrange
            var vital = new Vital
            {
                PatientId = 1,
                Temperature = temperature
            };

            var patient = new Patient
            {
                Id = 1,
                DateOfBirth = DateTime.Today.AddYears(-30)
            };

            _mockPatientDbSet.Setup(d => d.FindAsync(1))
                .ReturnsAsync(patient);

            // Act
            var result = await _vitalService.ValidateVitalRangesAsync(vital);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetAbnormalVitalsAsync_ReturnsOnlyAbnormalVitals()
        {
            // Arrange
            var patientId = 1;
            var patient = new Patient
            {
                Id = patientId,
                DateOfBirth = DateTime.Today.AddYears(-30)
            };

            var vitals = new List<Vital>
            {
                new Vital { PatientId = patientId, Temperature = 98.6M }, // Normal
                new Vital { PatientId = patientId, Temperature = 103.0M }, // Abnormal
                new Vital { PatientId = patientId, Pulse = 150M } // Abnormal
            };

            _mockPatientDbSet.Setup(d => d.FindAsync(patientId))
                .ReturnsAsync(patient);

            var queryableVitals = vitals.AsQueryable();
            _mockVitalDbSet.As<IQueryable<Vital>>().Setup(m => m.Provider).Returns(queryableVitals.Provider);
            _mockVitalDbSet.As<IQueryable<Vital>>().Setup(m => m.Expression).Returns(queryableVitals.Expression);
            _mockVitalDbSet.As<IQueryable<Vital>>().Setup(m => m.ElementType).Returns(queryableVitals.ElementType);
            _mockVitalDbSet.As<IQueryable<Vital>>().Setup(m => m.GetEnumerator()).Returns(queryableVitals.GetEnumerator());

            // Act
            var result = await _vitalService.GetAbnormalVitalsAsync(patientId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, v => v.Temperature == 103.0M);
            Assert.Contains(result, v => v.Pulse == 150M);
        }
    }
}
