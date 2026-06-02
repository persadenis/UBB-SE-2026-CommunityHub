using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.serviceInterfaces.Services
{
    public interface IMessageService
    {
        Task<string?> GetCannotSendReasonAsync(Guid conversationId, Guid userId);
        Task<List<Message>> GetMessagesAsync(Guid conversationId, Guid userId, int skip, int take);
        Task<Message> SendMessageAsync(Guid conversationId, Guid senderId, string content, Guid? replyToId);
        Task<string> PersistImageAttachmentAsync(string sourcePath);
        Task EditMessageAsync(Guid messageId, Guid requesterId, string newContent);
        Task DeleteMessageAsync(Guid messageId, Guid requesterId);
        Task SetNicknameAsync(Guid conversationId, Guid userId, string? nickname);
    }
}