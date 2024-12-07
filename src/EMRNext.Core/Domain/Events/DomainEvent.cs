using System;

namespace EMRNext.Core.Domain.Events
{
    /// <summary>
    /// Base class for domain events in the EMRNext system
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// Unique identifier for the event
        /// </summary>
        public Guid EventId { get; } = Guid.NewGuid();

        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates the type of event
        /// </summary>
        public abstract string EventType { get; }
    }

    /// <summary>
    /// Domain event for patient-related actions
    /// </summary>
    public class PatientDomainEvent : DomainEvent
    {
        public Guid PatientId { get; set; }
        public override string EventType => GetType().Name;
    }

    /// <summary>
    /// Event raised when a new patient is registered
    /// </summary>
    public class PatientRegisteredEvent : PatientDomainEvent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    /// <summary>
    /// Event raised when a patient's medical record is updated
    /// </summary>
    public class PatientMedicalRecordUpdatedEvent : PatientDomainEvent
    {
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Domain event handler interface
    /// </summary>
    public interface IDomainEventHandler<in T> where T : DomainEvent
    {
        /// <summary>
        /// Handles the domain event
        /// </summary>
        /// <param name="domainEvent">The domain event to handle</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task HandleAsync(T domainEvent);
    }

    /// <summary>
    /// Domain event dispatcher
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatch a domain event to its handlers
        /// </summary>
        /// <param name="domainEvent">The domain event to dispatch</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task DispatchAsync(DomainEvent domainEvent);
    }
}
