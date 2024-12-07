using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Services.Portal;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EMRNext.Core.Tests.Services
{
    public class GroupSeriesServiceTests
    {
        private readonly Mock<EMRNextDbContext> _mockContext;
        private readonly Mock<ILogger<GroupSeriesService>> _mockLogger;
        private readonly Mock<IResourceManagementService> _mockResourceService;
        private readonly Mock<IScheduleService> _mockScheduleService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly GroupSeriesService _service;
        private readonly Mock<DbSet<GroupSeries>> _mockSeriesDbSet;
        private readonly Mock<DbSet<ParticipantOutcome>> _mockOutcomesDbSet;

        public GroupSeriesServiceTests()
        {
            _mockContext = new Mock<EMRNextDbContext>();
            _mockLogger = new Mock<ILogger<GroupSeriesService>>();
            _mockResourceService = new Mock<IResourceManagementService>();
            _mockScheduleService = new Mock<IScheduleService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockDocumentService = new Mock<IDocumentService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockSeriesDbSet = new Mock<DbSet<GroupSeries>>();
            _mockOutcomesDbSet = new Mock<DbSet<ParticipantOutcome>>();

            _mockContext.Setup(c => c.GroupSeries).Returns(_mockSeriesDbSet.Object);
            _mockContext.Setup(c => c.ParticipantOutcomes).Returns(_mockOutcomesDbSet.Object);

            _service = new GroupSeriesService(
                _mockContext.Object,
                _mockLogger.Object,
                _mockResourceService.Object,
                _mockScheduleService.Object,
                _mockNotificationService.Object,
                _mockDocumentService.Object,
                _mockAuditService.Object);
        }

        [Fact]
        public async Task GetSeriesAsync_ExistingSeries_ReturnsSeries()
        {
            // Arrange
            var seriesId = 1;
            var expectedSeries = new GroupSeries { Id = seriesId, Name = "Test Series" };
            var seriesList = new List<GroupSeries> { expectedSeries };
            var queryableSeries = seriesList.AsQueryable();

            _mockSeriesDbSet.As<IQueryable<GroupSeries>>().Setup(m => m.Provider).Returns(queryableSeries.Provider);
            _mockSeriesDbSet.As<IQueryable<GroupSeries>>().Setup(m => m.Expression).Returns(queryableSeries.Expression);
            _mockSeriesDbSet.As<IQueryable<GroupSeries>>().Setup(m => m.ElementType).Returns(queryableSeries.ElementType);
            _mockSeriesDbSet.As<IQueryable<GroupSeries>>().Setup(m => m.GetEnumerator()).Returns(queryableSeries.GetEnumerator());

            // Act
            var result = await _service.GetSeriesAsync(seriesId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(seriesId, result.Id);
            Assert.Equal("Test Series", result.Name);
        }

        [Fact]
        public async Task UpdateSeriesAsync_ValidSeries_UpdatesSuccessfully()
        {
            // Arrange
            var seriesId = 1;
            var existingSeries = new GroupSeries 
            { 
                Id = seriesId, 
                Name = "Old Name",
                StartDate = DateTime.Today,
                SessionDurationMinutes = 60
            };
            var updatedSeries = new GroupSeries 
            { 
                Id = seriesId, 
                Name = "New Name",
                StartDate = DateTime.Today.AddDays(1),
                SessionDurationMinutes = 90
            };

            _mockSeriesDbSet.Setup(d => d.FindAsync(seriesId)).ReturnsAsync(existingSeries);

            // Act
            var result = await _service.UpdateSeriesAsync(updatedSeries);

            // Assert
            Assert.NotNull(result);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
            _mockAuditService.Verify(a => a.LogActivityAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task DeleteSeriesAsync_ExistingSeries_DeletesSuccessfully()
        {
            // Arrange
            var seriesId = 1;
            var series = new GroupSeries 
            { 
                Id = seriesId, 
                Name = "Test Series",
                Participants = new List<SeriesParticipant>()
            };

            _mockSeriesDbSet.Setup(d => d.FindAsync(seriesId)).ReturnsAsync(series);

            // Act
            var result = await _service.DeleteSeriesAsync(seriesId);

            // Assert
            Assert.True(result);
            Assert.Equal("Deleted", series.Status);
            Assert.NotNull(series.DeletedDate);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
            _mockAuditService.Verify(a => a.LogActivityAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task AdjustForHolidaysAsync_WithHolidayConflicts_AdjustsSchedule()
        {
            // Arrange
            var seriesId = 1;
            var series = new GroupSeries { Id = seriesId, Name = "Test Series" };
            var upcomingSessions = new List<GroupAppointment>
            {
                new GroupAppointment { StartTime = DateTime.Today.AddDays(1) },
                new GroupAppointment { StartTime = DateTime.Today.AddDays(2) }
            };

            _mockSeriesDbSet.Setup(d => d.FindAsync(seriesId)).ReturnsAsync(series);
            _mockScheduleService.Setup(s => s.IsHolidayAsync(It.IsAny<DateTime>())).ReturnsAsync(true);
            _mockScheduleService.Setup(s => s.GetNextAvailableDateAsync(It.IsAny<DateTime>()))
                .ReturnsAsync((DateTime d) => d.AddDays(1));

            // Act
            var result = await _service.AdjustForHolidaysAsync(seriesId);

            // Assert
            Assert.True(result);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
            _mockNotificationService.Verify(n => n.NotifyScheduleChangesAsync(
                It.IsAny<GroupSeries>(),
                It.IsAny<IEnumerable<GroupAppointment>>()), Times.Once);
        }

        [Fact]
        public async Task GetParticipantOutcomesAsync_WithOutcomes_ReturnsOutcomes()
        {
            // Arrange
            var seriesId = 1;
            var outcomes = new List<ParticipantOutcome>
            {
                new ParticipantOutcome { GroupSeriesId = seriesId, ParticipantId = 1 },
                new ParticipantOutcome { GroupSeriesId = seriesId, ParticipantId = 2 }
            };
            var queryableOutcomes = outcomes.AsQueryable();

            _mockOutcomesDbSet.As<IQueryable<ParticipantOutcome>>().Setup(m => m.Provider).Returns(queryableOutcomes.Provider);
            _mockOutcomesDbSet.As<IQueryable<ParticipantOutcome>>().Setup(m => m.Expression).Returns(queryableOutcomes.Expression);
            _mockOutcomesDbSet.As<IQueryable<ParticipantOutcome>>().Setup(m => m.ElementType).Returns(queryableOutcomes.ElementType);
            _mockOutcomesDbSet.As<IQueryable<ParticipantOutcome>>().Setup(m => m.GetEnumerator()).Returns(queryableOutcomes.GetEnumerator());

            // Act
            var result = await _service.GetParticipantOutcomesAsync(seriesId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GenerateParticipantReportAsync_ValidParticipant_GeneratesReport()
        {
            // Arrange
            var seriesId = 1;
            var patientId = 1;
            var participant = new SeriesParticipant 
            { 
                GroupSeriesId = seriesId, 
                PatientId = patientId 
            };
            var expectedReport = new byte[] { 1, 2, 3 };

            _mockContext.Setup(c => c.SeriesParticipants.FindAsync(seriesId, patientId))
                .ReturnsAsync(participant);
            _mockDocumentService.Setup(d => d.GenerateParticipantReportAsync(It.IsAny<ParticipantReportData>()))
                .ReturnsAsync(expectedReport);

            // Act
            var result = await _service.GenerateParticipantReportAsync(seriesId, patientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedReport, result);
            _mockDocumentService.Verify(d => d.GenerateParticipantReportAsync(
                It.IsAny<ParticipantReportData>()), Times.Once);
        }

        [Fact]
        public async Task GetSeriesAsync_NonExistentSeries_ReturnsNull()
        {
            // Arrange
            var seriesId = 999;
            var seriesList = new List<GroupSeries>();
            var queryableSeries = seriesList.AsQueryable();

            SetupMockDbSet(_mockSeriesDbSet, queryableSeries);

            // Act
            var result = await _service.GetSeriesAsync(seriesId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateSeriesAsync_NonExistentSeries_ThrowsNotFoundException()
        {
            // Arrange
            var series = new GroupSeries { Id = 999, Name = "Non-existent Series" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.UpdateSeriesAsync(series));
        }

        [Fact]
        public async Task UpdateSeriesAsync_NullSeries_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _service.UpdateSeriesAsync(null));
        }

        [Fact]
        public async Task DeleteSeriesAsync_NonExistentSeries_ReturnsFalse()
        {
            // Arrange
            var seriesId = 999;
            _mockSeriesDbSet.Setup(d => d.FindAsync(seriesId)).ReturnsAsync((GroupSeries)null);

            // Act
            var result = await _service.DeleteSeriesAsync(seriesId);

            // Assert
            Assert.False(result);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task AdjustForHolidaysAsync_NoHolidayConflicts_ReturnsTrueWithoutChanges()
        {
            // Arrange
            var seriesId = 1;
            var series = new GroupSeries { Id = seriesId, Name = "Test Series" };
            var upcomingSessions = new List<GroupAppointment>
            {
                new GroupAppointment { StartTime = DateTime.Today.AddDays(1) }
            };

            _mockSeriesDbSet.Setup(d => d.FindAsync(seriesId)).ReturnsAsync(series);
            _mockScheduleService.Setup(s => s.IsHolidayAsync(It.IsAny<DateTime>())).ReturnsAsync(false);

            // Act
            var result = await _service.AdjustForHolidaysAsync(seriesId);

            // Assert
            Assert.True(result);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Never);
            _mockNotificationService.Verify(n => n.NotifyScheduleChangesAsync(
                It.IsAny<GroupSeries>(),
                It.IsAny<IEnumerable<GroupAppointment>>()), Times.Never);
        }

        [Fact]
        public async Task RecordSeriesOutcomeAsync_ParticipantNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var seriesId = 1;
            var patientId = 999;
            var outcome = new ParticipantOutcome();

            _mockContext.Setup(c => c.SeriesParticipants.FindAsync(seriesId, patientId))
                .ReturnsAsync((SeriesParticipant)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.RecordSeriesOutcomeAsync(seriesId, patientId, outcome));
        }

        [Fact]
        public async Task GenerateParticipantReportAsync_ParticipantNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var seriesId = 1;
            var patientId = 999;

            _mockContext.Setup(c => c.SeriesParticipants.FindAsync(seriesId, patientId))
                .ReturnsAsync((SeriesParticipant)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.GenerateParticipantReportAsync(seriesId, patientId));
        }

        [Fact]
        public async Task WithdrawParticipantAsync_ValidParticipant_UpdatesStatusAndCancelsFutureSessions()
        {
            // Arrange
            var seriesId = 1;
            var patientId = 1;
            var participant = new SeriesParticipant 
            { 
                GroupSeriesId = seriesId, 
                PatientId = patientId,
                EnrollmentStatus = "Active"
            };

            _mockContext.Setup(c => c.SeriesParticipants.FindAsync(seriesId, patientId))
                .ReturnsAsync(participant);

            // Act
            var result = await _service.WithdrawParticipantAsync(seriesId, patientId);

            // Assert
            Assert.True(result);
            Assert.Equal("Withdrawn", participant.EnrollmentStatus);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
            _mockAuditService.Verify(a => a.LogActivityAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>()), Times.Once);
        }

        private void SetupMockDbSet<T>(Mock<DbSet<T>> mockDbSet, IQueryable<T> data) where T : class
        {
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        }
    }
}
