using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMRNext.Infrastructure.Services.External
{
    public interface ITelehealthService
    {
        Task<Session> CreateSessionAsync(SessionRequest request);
        Task<Session> GetSessionAsync(string sessionId);
        Task<bool> EndSessionAsync(string sessionId);
        Task<bool> UpdateSessionAsync(string sessionId, SessionUpdateRequest request);
        Task<IEnumerable<Session>> GetSessionsByProviderAsync(string providerId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Session>> GetSessionsByPatientAsync(string patientId, DateTime startDate, DateTime endDate);
        Task<SessionToken> GenerateTokenAsync(string sessionId, string participantId, string role);
    }

    public class SessionRequest
    {
        public string ProviderId { get; set; }
        public string PatientId { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string SessionType { get; set; }
        public string AppointmentId { get; set; }
        public bool EnableRecording { get; set; }
        public SessionSettings Settings { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class Session
    {
        public string SessionId { get; set; }
        public string ProviderId { get; set; }
        public string PatientId { get; set; }
        public string Status { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string SessionType { get; set; }
        public string AppointmentId { get; set; }
        public bool RecordingEnabled { get; set; }
        public string RecordingUrl { get; set; }
        public SessionSettings Settings { get; set; }
        public List<SessionParticipant> Participants { get; set; }
        public SessionMetrics Metrics { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class SessionSettings
    {
        public bool EnableChat { get; set; }
        public bool EnableScreenShare { get; set; }
        public bool EnableWhiteboard { get; set; }
        public bool MuteParticipantsOnEntry { get; set; }
        public bool RequireParticipantApproval { get; set; }
        public int MaxParticipants { get; set; }
        public string WaitingRoomMessage { get; set; }
        public VideoQualitySettings VideoQuality { get; set; }
        public AudioQualitySettings AudioQuality { get; set; }
    }

    public class VideoQualitySettings
    {
        public string Resolution { get; set; }
        public int FrameRate { get; set; }
        public int Bitrate { get; set; }
    }

    public class AudioQualitySettings
    {
        public string Codec { get; set; }
        public int Bitrate { get; set; }
        public bool EnableEchoCancellation { get; set; }
        public bool EnableNoiseSuppression { get; set; }
    }

    public class SessionParticipant
    {
        public string ParticipantId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public DateTime JoinTime { get; set; }
        public DateTime? LeaveTime { get; set; }
        public string ConnectionStatus { get; set; }
        public DeviceInfo Device { get; set; }
        public NetworkStats Network { get; set; }
    }

    public class DeviceInfo
    {
        public string Type { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public bool HasCamera { get; set; }
        public bool HasMicrophone { get; set; }
        public bool HasSpeakers { get; set; }
    }

    public class NetworkStats
    {
        public string ConnectionType { get; set; }
        public int Bitrate { get; set; }
        public int PacketLoss { get; set; }
        public int Latency { get; set; }
        public string Quality { get; set; }
    }

    public class SessionMetrics
    {
        public int TotalParticipants { get; set; }
        public int MaxConcurrentParticipants { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public int ChatMessagesCount { get; set; }
        public bool RecordingAvailable { get; set; }
        public long RecordingSize { get; set; }
        public IDictionary<string, int> ParticipantDurations { get; set; }
        public List<SessionEvent> Events { get; set; }
    }

    public class SessionEvent
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string ParticipantId { get; set; }
        public IDictionary<string, string> Data { get; set; }
    }

    public class SessionUpdateRequest
    {
        public DateTime? ScheduledStartTime { get; set; }
        public int? DurationMinutes { get; set; }
        public bool? EnableRecording { get; set; }
        public SessionSettings Settings { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class SessionToken
    {
        public string Token { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string SessionId { get; set; }
        public string ParticipantId { get; set; }
        public string Role { get; set; }
    }
}
