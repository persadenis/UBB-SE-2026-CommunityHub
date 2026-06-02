using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.services
{
    public interface IProfileService
    {
        Task<List<User>> GetAllUsersAsync(Guid viewerUserId, string? searchQuery);
        Task<List<User>> GetMutualFriendsAsync(Guid userId1, Guid userId2);
        Task<User?> GetProfileAsync(Guid targetUserId);
        Task UpdateProfileAsync(Guid userId, string? bio, string? avatarUrl, DateTime? birthday);
        Task UpdateStatusAsync(Guid userId, UserStatus status);
    }
}