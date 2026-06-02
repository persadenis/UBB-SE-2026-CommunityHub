using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.services
{
    public interface IDirectMessageService
    {
        Task ClearExpiredPinAsync(Guid conversationId, Guid pinnedMessageId);
        Task<Conversation> GetOrCreateAsync(Guid userId1, Guid userId2);
        Task<Participant?> GetOtherParticipantAsync(Guid conversationId, Guid currentUserId);
        Task<User?> GetOtherUserAsync(Guid conversationId, Guid viewerUserId);
        Task<bool> IsBlockedAsync(Guid conversationId, Guid viewerUserId);
        Task<(Message Pinned, Message Notice)> PinMessageAsync(Guid conversationId, Guid requesterId, Guid messageId, DateTime expiresAt);
        Task<Message> UnpinMessageAsync(Guid conversationId, Guid requesterId);
    }
}