namespace EMRNext.Core.Interfaces
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string UserName { get; }
        string[] Roles { get; }
        bool IsAuthenticated { get; }
        string IpAddress { get; }
        string TimeZone { get; }
        string Culture { get; }
        string DeviceId { get; }
        string SessionId { get; }
    }
}
