using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.repoInterfaces.Repositories
{
    public interface IMessageRepository
    {
        Task<int> CountReadByAsync(Guid ConversationId, Guid MessageId);
        Task<int> CountUnreadAsync(Guid conversationId, Guid lastReadMessageId, Guid userId);
        Task<int> CountUnreadFromStartAsync(Guid conversationId, Guid userId);
        Task CreateAsync(Message m);
        Task DeleteByConversationAsync(Guid conversationId);
        Task<List<Message>> GetAllForConversationAsync(Guid conversationId);
        Task<List<Message>> GetByConversationAsync(Guid conversationId, int skip, int take);
        Task<Message?> GetByIdAsync(Guid id);
        Task<Message?> GetLastMessageAsync(Guid conversationId);
        Task<Guid?> GetLatestReadableMessageIdAsync(Guid conversationId, Guid userId);
        Task<List<Message>> GetReactionsForMessageAsync(Guid parentMessageId);
        Task<List<Guid>> GetReadByUserIdsAsync(Guid ConversationId, Guid MessageId);
        Task<List<Message>> GetSystemMessagesAsync(Guid conversationId);
        Task<List<Message>> SearchInConversationAsync(Guid conversationId, string query);
        Task SetEditedAsync(Guid id);
        Task SetPinExpiresAtAsync(Guid id, DateTime? expiresAt);
        Task SoftDeleteAsync(Guid id);
        Task UnsoftDeleteAsync(Guid id);
        Task UpdateContentAsync(Guid id, string newContent);
    }
}