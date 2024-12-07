using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.Portal
{
    public interface ISecureMessagingService
    {
        Task<SecureMessage> SendMessageAsync(int senderId, int recipientId, string subject, string content, bool isUrgent = false);
        Task<SecureMessage> ReplyToMessageAsync(int originalMessageId, int senderId, string content);
        Task<bool> MarkAsReadAsync(int messageId, int userId);
        Task<IEnumerable<SecureMessage>> GetInboxMessagesAsync(int userId, int skip = 0, int take = 20);
        Task<IEnumerable<SecureMessage>> GetSentMessagesAsync(int userId, int skip = 0, int take = 20);
        Task<SecureMessage> GetMessageByIdAsync(int messageId, int userId);
        Task<bool> DeleteMessageAsync(int messageId, int userId);
        Task<SecureMessageAttachment> AddAttachmentAsync(int messageId, string fileName, string contentType, byte[] content);
        Task<SecureMessageAttachment> GetAttachmentAsync(int attachmentId, int userId);
        Task<bool> DeleteAttachmentAsync(int attachmentId, int userId);
        Task<IEnumerable<SecureMessage>> SearchMessagesAsync(int userId, string searchTerm, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> ArchiveMessageAsync(int messageId, int userId);
        Task<bool> UnarchiveMessageAsync(int messageId, int userId);
        Task<bool> SetMessageUrgencyAsync(int messageId, int userId, bool isUrgent);
        Task<int> GetUnreadMessageCountAsync(int userId);
    }
}
