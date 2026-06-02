using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;

namespace ChatAndEvents.Data.ChatData.services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFriendRepository _friendRepository;

        public ProfileService(IUserRepository userRepository, IFriendRepository friendRepository)
        {
            this._userRepository = userRepository;
            this._friendRepository = friendRepository;
        }

        public async Task<User?> GetProfileAsync(Guid targetUserId)
        {
            return await _userRepository.GetByIdAsync(targetUserId);
        }

        public async Task<List<User>> GetAllUsersAsync(Guid viewerUserId, string? searchQuery)
        {
            var users = string.IsNullOrWhiteSpace(searchQuery)
                ? await _userRepository.GetAllAsync()
                : await _userRepository.SearchByUsernameAsync(searchQuery);

            return users.Where(user => user.Id != viewerUserId).ToList();
        }

        public async Task UpdateProfileAsync(Guid userId, string? bio, string? avatarUrl, DateTime? birthday)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            if (bio != null)
            {
                user.Bio = bio;
            }

            if (avatarUrl != null)
            {
                user.AvatarUrl = avatarUrl;
            }

            if (birthday != null)
            {
                user.Birthday = birthday;
            }

            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdateStatusAsync(Guid userId, UserStatus status)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            user.Status = status;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<List<User>> GetMutualFriendsAsync(Guid userId1, Guid userId2)
        {
            var mutualFriendIds = await _friendRepository.GetMutualFriendIdentifiersAsync(userId1, userId2);
            var mutualFriends = new List<User>();

            foreach (var mutualFriendId in mutualFriendIds)
            {
                var user = await _userRepository.GetByIdAsync(mutualFriendId);
                if (user != null)
                {
                    mutualFriends.Add(user);
                }
            }

            return mutualFriends;
        }
    }
}
