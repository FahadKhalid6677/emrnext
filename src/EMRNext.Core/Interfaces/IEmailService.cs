using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailAsync(string[] to, string subject, string body);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath);
        Task SendTemplatedEmailAsync(string to, string templateName, object templateData);
        bool ValidateEmailAddress(string email);
    }
}
