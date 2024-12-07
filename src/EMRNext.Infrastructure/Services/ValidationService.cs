using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMRNext.Infrastructure.Services
{
    public class ValidationService : IValidationService
    {
        private readonly EMRNextDbContext _context;

        public ValidationService(EMRNextDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return false;

            // Check if the appointment time is valid
            if (appointment.StartTime >= appointment.EndTime)
                return false;

            // Check if the appointment overlaps with other appointments
            var overlapping = await _context.Appointments
                .Where(a => a.Id != appointmentId &&
                           a.ProviderId == appointment.ProviderId &&
                           a.StartTime < appointment.EndTime &&
                           a.EndTime > appointment.StartTime)
                .AnyAsync();

            if (overlapping)
                return false;

            return true;
        }

        public async Task<bool> ValidateProviderScheduleAsync(int providerId, DateTime date)
        {
            var schedule = await _context.ProviderSchedules
                .FirstOrDefaultAsync(s => s.ProviderId == providerId && 
                                        s.Date.Date == date.Date);

            if (schedule == null)
                return false;

            // Check if the provider is available on this date
            if (!schedule.IsAvailable)
                return false;

            return true;
        }

        public async Task<bool> ValidatePatientEligibilityAsync(int patientId, string serviceType)
        {
            var patient = await _context.Patients
                .Include(p => p.Insurance)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null || patient.Insurance == null)
                return false;

            // Check if insurance is active
            if (!patient.Insurance.IsActive)
                return false;

            // Check if service is covered
            var isCovered = await _context.InsuranceCoverages
                .AnyAsync(c => c.InsuranceId == patient.Insurance.Id && 
                              c.ServiceType == serviceType);

            return isCovered;
        }

        public async Task<bool> ValidateReferralAsync(int referralId)
        {
            var referral = await _context.Referrals.FindAsync(referralId);
            if (referral == null) return false;

            // Check if referral has expired
            if (referral.ExpirationDate < DateTime.UtcNow)
                return false;

            // Check if all required visits have been used
            if (referral.RemainingVisits <= 0)
                return false;

            return true;
        }

        public async Task<bool> ValidateAuthorizationAsync(int authorizationId)
        {
            var authorization = await _context.Authorizations.FindAsync(authorizationId);
            if (authorization == null) return false;

            // Check if authorization is still valid
            if (authorization.ExpirationDate < DateTime.UtcNow)
                return false;

            // Check if authorization has remaining units
            if (authorization.RemainingUnits <= 0)
                return false;

            return true;
        }

        public async Task<Dictionary<string, string>> ValidatePatientDemographicsAsync(int patientId)
        {
            var errors = new Dictionary<string, string>();
            var patient = await _context.Patients.FindAsync(patientId);

            if (patient == null)
            {
                errors.Add("patient", "Patient not found");
                return errors;
            }

            if (string.IsNullOrEmpty(patient.FirstName))
                errors.Add("firstName", "First name is required");

            if (string.IsNullOrEmpty(patient.LastName))
                errors.Add("lastName", "Last name is required");

            if (patient.DateOfBirth == default)
                errors.Add("dateOfBirth", "Date of birth is required");

            if (string.IsNullOrEmpty(patient.PhoneNumber))
                errors.Add("phoneNumber", "Phone number is required");

            return errors;
        }
    }
}
