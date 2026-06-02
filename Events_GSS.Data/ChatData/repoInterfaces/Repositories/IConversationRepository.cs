using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.repoInterfaces.Repositories
{
    public interface IConversationRepository
    {
        Task CreateAsync(Conversation c);
        Task DeleteAsync(Guid conversationId);
        Task<List<Conversation>> GetAllForUserAsync(Guid userId);
        Task<Conversation?> GetByIdAsync(Guid id);
        Task<Conversation?> GetDmBetweenAsync(Guid userId1, Guid userId2);
        Task SetPinnedMessageAsync(Guid conversationId, Guid? messageId);
        Task UpdateAsync(Conversation c);
    }
}