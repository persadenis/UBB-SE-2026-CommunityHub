using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.repoInterfaces.Repositories
{
    public interface IFriendRepository
    {
        Task<Friend?> GetFriendshipAsync(Guid firstUserId, Guid secondUserId);
        Task<List<Friend>> GetAllFriendshipsForUserAsync(Guid targetUserId);
        Task<List<Friend>> GetPendingRequestsForUserAsync(Guid targetUserId);
        Task<List<Friend>> GetAcceptedFriendsAsync(Guid targetUserId);
        Task<List<Guid>> GetMutualFriendIdentifiersAsync(Guid firstUserId, Guid secondUserId);
        Task<bool> CheckIfFriendsAsync(Guid firstUserId, Guid secondUserId);
        Task CreateFriendshipAsync(Friend newFriendship);
        Task UpdateFriendshipStatusAsync(Guid firstUserId, Guid secondUserId, FriendStatus newStatus);
        Task SetMatchStatusAsync(Guid firstUserId, Guid secondUserId, bool isMatchStatus);
        Task DeleteFriendshipAsync(Guid firstUserId, Guid secondUserId);
    }
}
