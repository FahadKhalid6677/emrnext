using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models
{
    public class PatientPortal
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
        public List<PortalPermission> Permissions { get; set; }
    }

    public class HealthGoal
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public HealthGoalType Type { get; set; }
        public HealthGoalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public List<HealthGoalProgress> ProgressTracking { get; set; }
    }

    public class HealthGoalProgress
    {
        public Guid Id { get; set; }
        public Guid HealthGoalId { get; set; }
        public DateTime Timestamp { get; set; }
        public double ProgressValue { get; set; }
        public string Notes { get; set; }
    }

    public class PatientReminder
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ReminderType Type { get; set; }
        public DateTime ReminderDateTime { get; set; }
        public bool IsRepeating { get; set; }
        public ReminderFrequency? RepeatFrequency { get; set; }
        public ReminderStatus Status { get; set; }
        public List<ReminderNotification> Notifications { get; set; }
    }

    public class ReminderNotification
    {
        public Guid Id { get; set; }
        public Guid ReminderId { get; set; }
        public NotificationChannel Channel { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDelivered { get; set; }
    }

    public class PatientEducationResource
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public EducationResourceType Type { get; set; }
        public string ContentUrl { get; set; }
        public List<string> Tags { get; set; }
        public List<string> RecommendedFor { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PatientSurvey
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Title { get; set; }
        public SurveyType Type { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<SurveyQuestion> Questions { get; set; }
        public Dictionary<string, string> Responses { get; set; }
        public SurveyStatus Status { get; set; }
    }

    public class SurveyQuestion
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public SurveyQuestionType QuestionType { get; set; }
        public List<string> Options { get; set; }
        public bool IsRequired { get; set; }
    }

    public enum PortalPermission
    {
        ViewMedicalRecords,
        ScheduleAppointments,
        MessageProviders,
        ViewBillingInfo,
        UpdatePersonalInfo
    }

    public enum HealthGoalType
    {
        WeightManagement,
        BloodPressureControl,
        DiabetesManagement,
        ExerciseRoutine,
        NutritionImprovement
    }

    public enum HealthGoalStatus
    {
        InProgress,
        Achieved,
        Discontinued,
        Pending
    }

    public enum ReminderType
    {
        Medication,
        Appointment,
        HealthScreening,
        Vaccination,
        LifestyleGoal
    }

    public enum ReminderFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Annually
    }

    public enum ReminderStatus
    {
        Pending,
        Completed,
        Missed,
        Snoozed
    }

    public enum NotificationChannel
    {
        Email,
        SMS,
        InAppNotification,
        Push
    }

    public enum EducationResourceType
    {
        Article,
        Video,
        Infographic,
        Podcast,
        InteractiveGuide
    }

    public enum SurveyType
    {
        HealthAssessment,
        PatientSatisfaction,
        SymptomTracking,
        QualityOfLife,
        TreatmentFeedback
    }

    public enum SurveyQuestionType
    {
        MultipleChoice,
        SingleChoice,
        Rating,
        Text,
        YesNo
    }

    public enum SurveyStatus
    {
        Issued,
        InProgress,
        Completed,
        Expired
    }
}
