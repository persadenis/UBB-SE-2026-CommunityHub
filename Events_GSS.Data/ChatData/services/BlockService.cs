using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class BlockService : IBlockService
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IUserRepository _userRepository;

        public BlockService(IFriendRepository friendRepository, IUserRepository userRepository)
        {
            this._friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
            this._userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task BlockUserAsync(Guid blockerUserId, Guid targetUserId)
        {
            var friendshipRelation = await _friendRepository.GetFriendshipAsync(blockerUserId, targetUserId);
            if (friendshipRelation == null)
            {
                var newBlockedRelation = new Friend
                {
                    Id = Guid.NewGuid(),
                    UserId1 = blockerUserId,
                    UserId2 = targetUserId,
                    Status = FriendStatus.Blocked,
                    IsMatch = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _friendRepository.CreateFriendshipAsync(newBlockedRelation);

                return;
            }

            if (friendshipRelation.Status == FriendStatus.Accepted)
            {
                await _friendRepository.SetMatchStatusAsync(blockerUserId, targetUserId, true);
            }
            else if (friendshipRelation.Status == FriendStatus.Pending)
            {
                await _friendRepository.SetMatchStatusAsync(blockerUserId, targetUserId, false);
            }

            await _friendRepository.UpdateFriendshipStatusAsync(blockerUserId, targetUserId, FriendStatus.Blocked);
        }

        public async Task UnblockUserAsync(Guid blockerUserId, Guid targetUserId)
        {
            var friendshipRelation = await _friendRepository.GetFriendshipAsync(blockerUserId, targetUserId);
            if (friendshipRelation == null)
            {
                return;
            }

            if (friendshipRelation.Status == FriendStatus.Blocked)
            {
                var restoredFriendStatus = friendshipRelation.IsMatch ? FriendStatus.Accepted : FriendStatus.Pending;
                await _friendRepository.UpdateFriendshipStatusAsync(blockerUserId, targetUserId, restoredFriendStatus);
                return;
            }

            await _friendRepository.DeleteFriendshipAsync(blockerUserId, targetUserId);
        }

        public async Task<List<User>> GetBlockedUsersAsync(Guid targetUserId)
        {
            var blockedUserList = new List<User>();
            var allFriendshipRelations = await _friendRepository.GetAllFriendshipsForUserAsync(targetUserId);

            foreach (var friendshipRelation in allFriendshipRelations)
            {
                if (friendshipRelation.Status != FriendStatus.Blocked)
                {
                    continue;
                }

                var otherUserIdentifier = friendshipRelation.UserId1 == targetUserId ? friendshipRelation.UserId2 : friendshipRelation.UserId1;
                var userObject = await _userRepository.GetByIdAsync(otherUserIdentifier);

                if (userObject != null)
                {
                    blockedUserList.Add(userObject);
                }
            }

            return blockedUserList;
        }

        public async Task<bool> IsBlockedAsync(Guid blockerId, Guid targetId)
        {
            var relation = await _friendRepository.GetFriendshipAsync(blockerId, targetId);
            return relation != null && relation.Status == FriendStatus.Blocked;
        }

        public async Task<bool> CheckIfBlockedAsync(Guid blockerUserId, Guid targetUserId)
        {
            var friendshipRelation = await _friendRepository.GetFriendshipAsync(blockerUserId, targetUserId);
            return friendshipRelation != null && friendshipRelation.Status == FriendStatus.Blocked;
        }
    }
}
