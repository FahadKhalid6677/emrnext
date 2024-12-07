using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Exceptions;

namespace EMRNext.IntegrationTests.Services
{
    public class SchedulingServiceTests : IClassFixture<TestFixture>
    {
        private readonly ISchedulingService _schedulingService;
        private readonly ISchedulingRepository _schedulingRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly ILoggingService _loggingService;

        public SchedulingServiceTests(TestFixture fixture)
        {
            _schedulingService = fixture.ServiceProvider.GetRequiredService<ISchedulingService>();
            _schedulingRepository = fixture.ServiceProvider.GetRequiredService<ISchedulingRepository>();
            _providerRepository = fixture.ServiceProvider.GetRequiredService<IProviderRepository>();
            _loggingService = fixture.ServiceProvider.GetRequiredService<ILoggingService>();
        }

        [Fact]
        public async Task ScheduleAppointment_WithValidData_ShouldCreateAppointment()
        {
            // Arrange
            var provider = await CreateTestProvider();
            var appointment = new Appointment
            {
                PatientId = 1,
                ProviderId = provider.Id,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
                AppointmentType = "Follow-up",
                Location = "Main Clinic"
            };

            // Act
            var result = await _schedulingService.ScheduleAppointmentAsync(appointment);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Status.Should().Be(AppointmentStatus.Scheduled);
            result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task ScheduleAppointment_WithConflict_ShouldThrowBusinessRuleException()
        {
            // Arrange
            var provider = await CreateTestProvider();
            var existingAppointment = await CreateTestAppointment(provider.Id);
            
            var conflictingAppointment = new Appointment
            {
                PatientId = 2,
                ProviderId = provider.Id,
                StartTime = existingAppointment.StartTime,
                EndTime = existingAppointment.EndTime,
                AppointmentType = "Follow-up",
                Location = "Main Clinic"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => 
                _schedulingService.ScheduleAppointmentAsync(conflictingAppointment));
        }

        [Fact]
        public async Task CancelAppointment_WithValidReason_ShouldCancelAppointment()
        {
            // Arrange
            var provider = await CreateTestProvider();
            var appointment = await CreateTestAppointment(provider.Id);
            var reason = "Patient request";

            // Act
            var result = await _schedulingService.CancelAppointmentAsync(appointment.Id, reason);

            // Assert
            result.Should().BeTrue();
            var cancelledAppointment = await _schedulingRepository.GetAppointmentAsync(appointment.Id);
            cancelledAppointment.Status.Should().Be(AppointmentStatus.Cancelled);
            cancelledAppointment.CancellationReason.Should().Be(reason);
        }

        [Fact]
        public async Task CancelAppointment_TooCloseToStart_ShouldThrowBusinessRuleException()
        {
            // Arrange
            var provider = await CreateTestProvider();
            var appointment = new Appointment
            {
                PatientId = 1,
                ProviderId = provider.Id,
                StartTime = DateTime.UtcNow.AddHours(1), // Less than 24 hours notice
                EndTime = DateTime.UtcNow.AddHours(1).AddMinutes(30),
                AppointmentType = "Follow-up",
                Location = "Main Clinic",
                Status = AppointmentStatus.Scheduled
            };
            appointment = await _schedulingRepository.CreateAppointmentAsync(appointment);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => 
                _schedulingService.CancelAppointmentAsync(appointment.Id, "Patient request"));
        }

        [Fact]
        public async Task GetProviderAvailability_ShouldReturnAvailableSlots()
        {
            // Arrange
            var provider = await CreateTestProvider();
            var date = DateTime.UtcNow.Date.AddDays(1);

            // Act
            var slots = await _schedulingService.GetProviderAvailabilityAsync(provider.Id, date);

            // Assert
            slots.Should().NotBeNull();
            slots.Should().BeOfType<List<TimeSlot>>();
            slots.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetScheduleMetrics_ShouldReturnMetrics()
        {
            // Arrange
            var provider = await CreateTestProvider();
            await CreateTestAppointments(provider.Id);
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(7);

            // Act
            var metrics = await _schedulingService.GetScheduleMetricsAsync(start, end);

            // Assert
            metrics.Should().NotBeNull();
            metrics.TotalAppointments.Should().BeGreaterThan(0);
            metrics.UtilizationRate.Should().BeGreaterThanOrEqualTo(0);
            metrics.UtilizationRate.Should().BeLessThanOrEqualTo(100);
        }

        // Helper methods
        private async Task<Provider> CreateTestProvider()
        {
            var provider = new Provider
            {
                FirstName = "Test",
                LastName = "Provider",
                Specialty = "Family Medicine",
                Status = ProviderStatus.Active
            };
            return await _providerRepository.AddAsync(provider);
        }

        private async Task<Appointment> CreateTestAppointment(int providerId)
        {
            var appointment = new Appointment
            {
                PatientId = 1,
                ProviderId = providerId,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
                AppointmentType = "Follow-up",
                Location = "Main Clinic",
                Status = AppointmentStatus.Scheduled
            };
            return await _schedulingRepository.CreateAppointmentAsync(appointment);
        }

        private async Task CreateTestAppointments(int providerId)
        {
            // Create a mix of scheduled, completed, and cancelled appointments
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    PatientId = 1,
                    ProviderId = providerId,
                    StartTime = DateTime.UtcNow.AddDays(1),
                    EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
                    AppointmentType = "Follow-up",
                    Location = "Main Clinic",
                    Status = AppointmentStatus.Scheduled
                },
                new Appointment
                {
                    PatientId = 2,
                    ProviderId = providerId,
                    StartTime = DateTime.UtcNow.AddDays(-1),
                    EndTime = DateTime.UtcNow.AddDays(-1).AddMinutes(30),
                    AppointmentType = "New Patient",
                    Location = "Main Clinic",
                    Status = AppointmentStatus.Completed
                },
                new Appointment
                {
                    PatientId = 3,
                    ProviderId = providerId,
                    StartTime = DateTime.UtcNow.AddDays(2),
                    EndTime = DateTime.UtcNow.AddDays(2).AddMinutes(30),
                    AppointmentType = "Follow-up",
                    Location = "Main Clinic",
                    Status = AppointmentStatus.Cancelled,
                    CancellationReason = "Patient request"
                }
            };

            foreach (var appointment in appointments)
            {
                await _schedulingRepository.CreateAppointmentAsync(appointment);
            }
        }
    }
}
