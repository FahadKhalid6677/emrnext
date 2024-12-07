using System;
using System.Collections.Generic;
using EMRNext.Core.Models;

namespace EMRNext.Web.Models.API
{
    public class PatientPortalDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
        public List<PortalPermission> Permissions { get; set; }
    }

    public class HealthGoalDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public HealthGoalType Type { get; set; }
        public HealthGoalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public List<HealthGoalProgressDto> ProgressTracking { get; set; }
    }

    public class HealthGoalProgressDto
    {
        public Guid Id { get; set; }
        public Guid HealthGoalId { get; set; }
        public DateTime Timestamp { get; set; }
        public double ProgressValue { get; set; }
        public string Notes { get; set; }
    }

    public class PatientReminderDto
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
        public List<ReminderNotificationDto> Notifications { get; set; }
    }

    public class ReminderNotificationDto
    {
        public Guid Id { get; set; }
        public Guid ReminderId { get; set; }
        public NotificationChannel Channel { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDelivered { get; set; }
    }

    public class PatientEducationResourceDto
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

    public class PatientSurveyDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Title { get; set; }
        public SurveyType Type { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<SurveyQuestionDto> Questions { get; set; }
        public Dictionary<string, string> Responses { get; set; }
        public SurveyStatus Status { get; set; }
    }

    public class SurveyQuestionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public SurveyQuestionType QuestionType { get; set; }
        public List<string> Options { get; set; }
        public bool IsRequired { get; set; }
    }
}
