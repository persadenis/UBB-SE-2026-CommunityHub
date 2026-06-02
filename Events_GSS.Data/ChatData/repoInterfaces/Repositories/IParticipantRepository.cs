using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.repoInterfaces.Repositories
{
    public interface IParticipantRepository
    {
        Task CreateAsync(Participant participant);
        Task DeleteAsync(Guid conversationId, Guid userId);
        Task<List<Participant>> GetAllForConversationAsync(Guid conversationId);
        Task<List<Participant>> GetAllForUserAsync(Guid userId);
        Task<Participant?> GetAsync(Guid conversationId, Guid userId);
        Task UpdateFavouriteAsync(Guid conversationId, Guid userId, bool isFav);
        Task UpdateLastReadAsync(Guid conversationId, Guid userId, Guid messageId);
        Task UpdateNicknameAsync(Guid conversationId, Guid userId, string? nickname);
        Task UpdateRoleAsync(Guid conversationId, Guid userId, ParticipantRole role);
        Task UpdateTimeoutAsync(Guid conversationId, Guid userId, DateTime? until);
    }
}