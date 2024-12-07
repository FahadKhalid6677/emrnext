using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendSMSAsync(string phoneNumber, string message);
        Task SendInAppNotificationAsync(string userId, string message, string type = "info");
        Task<bool> VerifyDeliveryAsync(string notificationId);
    }
}
