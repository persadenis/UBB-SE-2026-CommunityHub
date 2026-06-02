using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.serviceInterfaces.Services
{
    public interface IMemberPanelService
    {
        Task<List<Participant>> GetMembersAsync(Guid conversationId);
        Task<List<Participant>> GetBannedMembersAsync(Guid conversationId);
        Task<List<User>> SearchUsersToAddAsync(Guid conversationId, string query);
        Task<User?> GetUserAsync(Guid userId);
    }
}