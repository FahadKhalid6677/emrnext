using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using EMRNext.Core.Services.Notifications;

namespace EMRNext.Core.Services
{
    public class PatientEngagementService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly INotificationService _notificationService;

        public PatientEngagementService(
            IPatientRepository patientRepository, 
            INotificationService notificationService)
        {
            _patientRepository = patientRepository;
            _notificationService = notificationService;
        }

        // Patient Portal Management
        public async Task<PatientPortal> CreatePatientPortalAccount(Guid patientId)
        {
            var existingPortal = await _patientRepository.GetPatientPortalByPatientId(patientId);
            if (existingPortal != null)
                throw new InvalidOperationException("Patient portal already exists");

            var newPortal = new PatientPortal
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                Username = GenerateUniqueUsername(patientId),
                IsActive = true,
                LastLogin = DateTime.UtcNow,
                Permissions = new List<PortalPermission>
                {
                    PortalPermission.ViewMedicalRecords,
                    PortalPermission.MessageProviders
                }
            };

            await _patientRepository.AddPatientPortal(newPortal);
            return newPortal;
        }

        private string GenerateUniqueUsername(Guid patientId)
        {
            return $"patient_{patientId.ToString().Substring(0, 8)}";
        }

        // Health Goal Management
        public async Task<HealthGoal> CreateHealthGoal(Guid patientId, HealthGoal goal)
        {
            goal.Id = Guid.NewGuid();
            goal.PatientId = patientId;
            goal.CreatedAt = DateTime.UtcNow;
            goal.Status = HealthGoalStatus.Pending;

            await _patientRepository.AddHealthGoal(goal);
            return goal;
        }

        public async Task<HealthGoalProgress> AddHealthGoalProgress(Guid healthGoalId, double progressValue, string notes)
        {
            var goal = await _patientRepository.GetHealthGoalById(healthGoalId);
            if (goal == null)
                throw new InvalidOperationException("Health goal not found");

            var progress = new HealthGoalProgress
            {
                Id = Guid.NewGuid(),
                HealthGoalId = healthGoalId,
                Timestamp = DateTime.UtcNow,
                ProgressValue = progressValue,
                Notes = notes
            };

            goal.ProgressTracking ??= new List<HealthGoalProgress>();
            goal.ProgressTracking.Add(progress);

            // Update goal status if progress meets certain criteria
            if (progressValue >= 100)
                goal.Status = HealthGoalStatus.Achieved;

            await _patientRepository.UpdateHealthGoal(goal);
            return progress;
        }

        // Reminder Management
        public async Task<PatientReminder> CreateReminder(Guid patientId, PatientReminder reminder)
        {
            reminder.Id = Guid.NewGuid();
            reminder.PatientId = patientId;
            reminder.Status = ReminderStatus.Pending;

            await _patientRepository.AddPatientReminder(reminder);
            await ScheduleReminderNotifications(reminder);

            return reminder;
        }

        private async Task ScheduleReminderNotifications(PatientReminder reminder)
        {
            var notifications = new List<ReminderNotification>();

            // Email notification
            var emailNotification = new ReminderNotification
            {
                Id = Guid.NewGuid(),
                ReminderId = reminder.Id,
                Channel = NotificationChannel.Email,
                SentAt = reminder.ReminderDateTime.AddHours(-24),
                IsDelivered = false
            };

            // SMS notification
            var smsNotification = new ReminderNotification
            {
                Id = Guid.NewGuid(),
                ReminderId = reminder.Id,
                Channel = NotificationChannel.SMS,
                SentAt = reminder.ReminderDateTime.AddHours(-2),
                IsDelivered = false
            };

            notifications.Add(emailNotification);
            notifications.Add(smsNotification);

            reminder.Notifications = notifications;
            await _patientRepository.UpdatePatientReminder(reminder);

            // Send actual notifications
            await _notificationService.SendReminderNotifications(reminder);
        }

        // Patient Education Resources
        public async Task<List<PatientEducationResource>> GetRecommendedResources(Guid patientId)
        {
            var patient = await _patientRepository.GetPatientById(patientId);
            var diagnoses = await _patientRepository.GetPatientDiagnoses(patientId);

            // Recommend resources based on patient's diagnoses and health conditions
            var recommendedResources = await _patientRepository.GetEducationResourcesByTags(
                diagnoses.Select(d => d.Name).ToList()
            );

            return recommendedResources;
        }

        // Patient Survey Management
        public async Task<PatientSurvey> IssueSurvey(Guid patientId, SurveyType surveyType)
        {
            var survey = new PatientSurvey
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                Title = GetSurveyTitle(surveyType),
                Type = surveyType,
                IssuedAt = DateTime.UtcNow,
                Status = SurveyStatus.Issued,
                Questions = GenerateSurveyQuestions(surveyType)
            };

            await _patientRepository.AddPatientSurvey(survey);
            await _notificationService.NotifySurveyIssued(survey);

            return survey;
        }

        private string GetSurveyTitle(SurveyType surveyType)
        {
            return surveyType switch
            {
                SurveyType.HealthAssessment => "Annual Health Assessment Survey",
                SurveyType.PatientSatisfaction => "Patient Care Experience Survey",
                SurveyType.SymptomTracking => "Symptom Monitoring Survey",
                SurveyType.QualityOfLife => "Quality of Life Evaluation",
                SurveyType.TreatmentFeedback => "Treatment Effectiveness Feedback",
                _ => "Patient Survey"
            };
        }

        private List<SurveyQuestion> GenerateSurveyQuestions(SurveyType surveyType)
        {
            return surveyType switch
            {
                SurveyType.HealthAssessment => new List<SurveyQuestion>
                {
                    new SurveyQuestion
                    {
                        Id = Guid.NewGuid(),
                        Text = "How would you rate your overall health?",
                        QuestionType = SurveyQuestionType.Rating,
                        IsRequired = true
                    },
                    // Add more questions
                },
                SurveyType.SymptomTracking => new List<SurveyQuestion>
                {
                    new SurveyQuestion
                    {
                        Id = Guid.NewGuid(),
                        Text = "Have you experienced any new symptoms?",
                        QuestionType = SurveyQuestionType.YesNo,
                        IsRequired = true
                    },
                    // Add more questions
                },
                // Add more survey types
                _ => new List<SurveyQuestion>()
            };
        }
    }
}
