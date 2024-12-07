using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Patients
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(AppointmentRequest request);
        Task<Appointment> GetAppointmentAsync(string id);
        Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(string patientId);
        Task<IEnumerable<Appointment>> GetProviderScheduleAsync(string providerId, DateTime date);
        Task<Appointment> UpdateAppointmentAsync(string id, AppointmentRequest request);
        Task<bool> CancelAppointmentAsync(string id, string reason);
        Task<IEnumerable<AppointmentSlot>> GetAvailableSlotsAsync(string providerId, DateTime date);
        Task<bool> CheckInPatientAsync(string appointmentId);
    }
}
