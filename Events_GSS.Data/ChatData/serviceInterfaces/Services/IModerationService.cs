using System;
using System.Threading.Tasks;

namespace ChatAndEvents.Data.ChatData.serviceInterfaces.Services
{
    public interface IModerationService
    {
        Task BanMemberAsync(Guid conversationId, Guid adminId, Guid targetId);
        Task UnbanMemberAsync(Guid conversationId, Guid adminId, Guid targetId);
        Task TimeoutMemberAsync(Guid conversationId, Guid adminId, Guid targetId, TimeSpan duration);
        Task RemoveTimeoutAsync(Guid conversationId, Guid adminId, Guid targetId);
        Task PromoteMemberAsync(Guid conversationId, Guid adminId, Guid targetId);
        Task DemoteMemberAsync(Guid conversationId, Guid adminId, Guid targetId);
        Task AddMemberAsync(Guid conversationId, Guid adminId, Guid newUserId);
    }
}