using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using EMRNext.Core.Services;

namespace EMRNext.Core.Services
{
    public class CareCoordinationService
    {
        private readonly IReferralRepository _referralRepository;
        private readonly ICareTransitionRepository _careTransitionRepository;
        private readonly ICareTeamRepository _careTeamRepository;
        private readonly ICommunicationLogRepository _communicationLogRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly INotificationService _notificationService;

        public CareCoordinationService(
            IReferralRepository referralRepository,
            ICareTransitionRepository careTransitionRepository,
            ICareTeamRepository careTeamRepository,
            ICommunicationLogRepository communicationLogRepository,
            IPatientRepository patientRepository,
            INotificationService notificationService)
        {
            _referralRepository = referralRepository;
            _careTransitionRepository = careTransitionRepository;
            _careTeamRepository = careTeamRepository;
            _communicationLogRepository = communicationLogRepository;
            _patientRepository = patientRepository;
            _notificationService = notificationService;
        }

        public async Task<Referral> CreateReferralAsync(Referral referral)
        {
            // Validate referral
            var patient = await _patientRepository.GetPatientByIdAsync(referral.PatientId);
            if (patient == null)
            {
                throw new ArgumentException("Invalid patient ID");
            }

            referral.Id = Guid.NewGuid();
            referral.CreatedAt = DateTime.UtcNow;
            referral.Status = ReferralStatus.Pending;

            // Create referral
            var createdReferral = await _referralRepository.AddReferralAsync(referral);

            // Notify receiving provider
            if (referral.ReceivingProviderId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    referral.ReceivingProviderId.Value, 
                    "New Referral", 
                    $"New referral received for patient {patient.Name} in {referral.Specialty}"
                );
            }

            return createdReferral;
        }

        public async Task<CareTransition> InitiateCareTransitionAsync(CareTransition transition)
        {
            // Validate care transition
            var patient = await _patientRepository.GetPatientByIdAsync(transition.PatientId);
            if (patient == null)
            {
                throw new ArgumentException("Invalid patient ID");
            }

            transition.Id = Guid.NewGuid();
            transition.TransitionDate = DateTime.UtcNow;
            transition.Status = CareTransitionStatus.Initiated;

            // Create care transition
            var createdTransition = await _careTransitionRepository.AddCareTransitionAsync(transition);

            // Notify providers involved in transition
            await _notificationService.SendNotificationAsync(
                transition.FromProviderId, 
                "Care Transition Initiated", 
                $"Care transition initiated for patient {patient.Name}"
            );

            await _notificationService.SendNotificationAsync(
                transition.ToProviderId, 
                "Care Transition Received", 
                $"Care transition received for patient {patient.Name}"
            );

            return createdTransition;
        }

        public async Task<CareTeam> CreateCareTeamAsync(CareTeam careTeam)
        {
            // Validate patient
            var patient = await _patientRepository.GetPatientByIdAsync(careTeam.PatientId);
            if (patient == null)
            {
                throw new ArgumentException("Invalid patient ID");
            }

            careTeam.Id = Guid.NewGuid();
            careTeam.CreatedAt = DateTime.UtcNow;

            // Ensure each team member has a unique ID
            foreach (var member in careTeam.Members)
            {
                member.Id = Guid.NewGuid();
                member.Status = CareTeamMemberStatus.Active;
            }

            // Create care team
            return await _careTeamRepository.AddCareTeamAsync(careTeam);
        }

        public async Task<CommunicationLog> SendCommunicationAsync(CommunicationLog communication)
        {
            communication.Id = Guid.NewGuid();
            communication.Timestamp = DateTime.UtcNow;
            communication.IsRead = false;

            // Send communication
            var sentCommunication = await _communicationLogRepository.AddCommunicationAsync(communication);

            // Notify receiver
            await _notificationService.SendNotificationAsync(
                communication.ReceiverId, 
                "New Communication", 
                $"You have a new {communication.Type} message"
            );

            return sentCommunication;
        }

        public async Task<List<Referral>> GetPatientReferralsAsync(Guid patientId)
        {
            return await _referralRepository.GetPatientReferralsAsync(patientId);
        }

        public async Task<List<CareTransition>> GetPatientCareTransitionsAsync(Guid patientId)
        {
            return await _careTransitionRepository.GetPatientCareTransitionsAsync(patientId);
        }

        public async Task<List<CommunicationLog>> GetProviderCommunicationsAsync(Guid providerId)
        {
            return await _communicationLogRepository.GetProviderCommunicationsAsync(providerId);
        }

        public async Task UpdateReferralStatusAsync(Guid referralId, ReferralStatus newStatus)
        {
            var referral = await _referralRepository.GetReferralByIdAsync(referralId);
            
            if (referral == null)
            {
                throw new ArgumentException("Referral not found");
            }

            referral.Status = newStatus;
            referral.UpdatedAt = DateTime.UtcNow;

            await _referralRepository.UpdateReferralAsync(referral);

            // Notify referring and receiving providers about status change
            await _notificationService.SendNotificationAsync(
                referral.ReferringProviderId, 
                "Referral Status Update", 
                $"Referral status changed to {newStatus}"
            );

            if (referral.ReceivingProviderId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    referral.ReceivingProviderId.Value, 
                    "Referral Status Update", 
                    $"Referral status changed to {newStatus}"
                );
            }
        }
    }
}
