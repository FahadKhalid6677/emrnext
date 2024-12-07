using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface ISMSService
    {
        Task SendSMSAsync(string phoneNumber, string message);
        Task SendSMSAsync(string[] phoneNumbers, string message);
        Task SendTemplatedSMSAsync(string phoneNumber, string templateName, object templateData);
        bool ValidatePhoneNumber(string phoneNumber);
        Task<bool> VerifyPhoneNumberAsync(string phoneNumber, string code);
        Task<string> GenerateVerificationCodeAsync(string phoneNumber);
    }
}
