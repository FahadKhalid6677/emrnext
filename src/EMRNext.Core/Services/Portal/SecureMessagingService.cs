using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Security;

namespace EMRNext.Core.Services.Portal
{
    public class SecureMessagingService : ISecureMessagingService
    {
        private readonly EMRNextDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public SecureMessagingService(
            EMRNextDbContext context,
            IEncryptionService encryptionService,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _context = context;
            _encryptionService = encryptionService;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<SecureMessage> SendMessageAsync(int senderId, int recipientId, string subject, string content, bool isUrgent = false)
        {
            var (encryptedContent, key, iv) = _encryptionService.EncryptMessage(content);

            var message = new SecureMessage
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Subject = subject,
                Content = encryptedContent,
                EncryptedKey = key,
                InitializationVector = iv,
                IsUrgent = isUrgent,
                CreatedDate = DateTime.UtcNow
            };

            _context.SecureMessages.Add(message);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                senderId,
                "MessageSent",
                $"Message sent to user {recipientId}",
                $"MessageId: {message.Id}"
            );

            await _notificationService.NotifyNewMessageAsync(recipientId, isUrgent);

            return message;
        }

        public async Task<SecureMessage> ReplyToMessageAsync(int originalMessageId, int senderId, string content)
        {
            var originalMessage = await _context.SecureMessages
                .FirstOrDefaultAsync(m => m.Id == originalMessageId);

            if (originalMessage == null)
            {
                throw new ArgumentException("Original message not found");
            }

            var (encryptedContent, key, iv) = _encryptionService.EncryptMessage(content);

            var reply = new SecureMessage
            {
                SenderId = senderId,
                RecipientId = originalMessage.SenderId,
                Subject = $"Re: {originalMessage.Subject}",
                Content = encryptedContent,
                EncryptedKey = key,
                InitializationVector = iv,
                ParentMessageId = originalMessageId,
                IsUrgent = originalMessage.IsUrgent,
                CreatedDate = DateTime.UtcNow
            };

            _context.SecureMessages.Add(reply);
            await _context.SaveChangesAsync();

            await _notificationService.NotifyNewMessageAsync(reply.RecipientId, reply.IsUrgent);

            return reply;
        }

        public async Task<bool> MarkAsReadAsync(int messageId, int userId)
        {
            var message = await _context.SecureMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.RecipientId == userId);

            if (message == null)
            {
                return false;
            }

            message.IsRead = true;
            message.ReadDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SecureMessage>> GetInboxMessagesAsync(int userId, int skip = 0, int take = 20)
        {
            return await _context.SecureMessages
                .Where(m => m.RecipientId == userId)
                .OrderByDescending(m => m.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<SecureMessage>> GetSentMessagesAsync(int userId, int skip = 0, int take = 20)
        {
            return await _context.SecureMessages
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.CreatedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<SecureMessage> GetMessageByIdAsync(int messageId, int userId)
        {
            var message = await _context.SecureMessages
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => 
                    m.Id == messageId && 
                    (m.SenderId == userId || m.RecipientId == userId));

            if (message == null)
            {
                return null;
            }

            if (message.RecipientId == userId && !message.IsRead)
            {
                await MarkAsReadAsync(messageId, userId);
            }

            message.Content = _encryptionService.DecryptMessage(
                message.Content,
                message.EncryptedKey,
                message.InitializationVector
            );

            return message;
        }

        public async Task<bool> DeleteMessageAsync(int messageId, int userId)
        {
            var message = await _context.SecureMessages
                .FirstOrDefaultAsync(m => 
                    m.Id == messageId && 
                    (m.SenderId == userId || m.RecipientId == userId));

            if (message == null)
            {
                return false;
            }

            _context.SecureMessages.Remove(message);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                userId,
                "MessageDeleted",
                $"Message {messageId} deleted",
                null
            );

            return true;
        }

        public async Task<SecureMessageAttachment> AddAttachmentAsync(int messageId, string fileName, string contentType, byte[] content)
        {
            var (encryptedContent, key, iv) = _encryptionService.EncryptFile(content);

            var attachment = new SecureMessageAttachment
            {
                MessageId = messageId,
                FileName = fileName,
                ContentType = contentType,
                EncryptedContent = Convert.ToBase64String(encryptedContent),
                EncryptedKey = key,
                InitializationVector = iv,
                FileSize = content.Length,
                UploadDate = DateTime.UtcNow
            };

            _context.SecureMessageAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return attachment;
        }

        public async Task<SecureMessageAttachment> GetAttachmentAsync(int attachmentId, int userId)
        {
            var attachment = await _context.SecureMessageAttachments
                .Include(a => a.Message)
                .FirstOrDefaultAsync(a => 
                    a.Id == attachmentId && 
                    (a.Message.SenderId == userId || a.Message.RecipientId == userId));

            if (attachment == null)
            {
                return null;
            }

            return attachment;
        }

        public async Task<bool> DeleteAttachmentAsync(int attachmentId, int userId)
        {
            var attachment = await _context.SecureMessageAttachments
                .Include(a => a.Message)
                .FirstOrDefaultAsync(a => 
                    a.Id == attachmentId && 
                    (a.Message.SenderId == userId || a.Message.RecipientId == userId));

            if (attachment == null)
            {
                return false;
            }

            _context.SecureMessageAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<SecureMessage>> SearchMessagesAsync(
            int userId, 
            string searchTerm, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var query = _context.SecureMessages
                .Where(m => m.SenderId == userId || m.RecipientId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(m => 
                    m.Subject.Contains(searchTerm) || 
                    m.Content.Contains(searchTerm));
            }

            if (startDate.HasValue)
            {
                query = query.Where(m => m.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.CreatedDate <= endDate.Value);
            }

            return await query
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> ArchiveMessageAsync(int messageId, int userId)
        {
            var message = await _context.SecureMessages
                .FirstOrDefaultAsync(m => 
                    m.Id == messageId && 
                    (m.SenderId == userId || m.RecipientId == userId));

            if (message == null)
            {
                return false;
            }

            // Implementation depends on how you want to handle archived messages
            // For example, you might have an IsArchived flag in the message entity

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnarchiveMessageAsync(int messageId, int userId)
        {
            var message = await _context.SecureMessages
                .FirstOrDefaultAsync(m => 
                    m.Id == messageId && 
                    (m.SenderId == userId || m.RecipientId == userId));

            if (message == null)
            {
                return false;
            }

            // Implementation depends on how you want to handle archived messages

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetMessageUrgencyAsync(int messageId, int userId, bool isUrgent)
        {
            var message = await _context.SecureMessages
                .FirstOrDefaultAsync(m => 
                    m.Id == messageId && 
                    m.SenderId == userId);

            if (message == null)
            {
                return false;
            }

            message.IsUrgent = isUrgent;
            await _context.SaveChangesAsync();

            if (isUrgent)
            {
                await _notificationService.NotifyUrgentMessageAsync(message.RecipientId, messageId);
            }

            return true;
        }

        public async Task<int> GetUnreadMessageCountAsync(int userId)
        {
            return await _context.SecureMessages
                .CountAsync(m => 
                    m.RecipientId == userId && 
                    !m.IsRead);
        }
    }
}
