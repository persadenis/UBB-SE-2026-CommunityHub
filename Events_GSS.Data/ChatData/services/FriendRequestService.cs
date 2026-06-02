using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.EventsData.Services.notificationServices;

namespace ChatAndEvents.Data.ChatData.services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly INotificationService? _notificationService;

        public FriendRequestService(
            IFriendRepository friendRepository,
            IUserRepository userRepository,
            IConversationRepository conversationRepository,
            IParticipantRepository participantRepository,
            INotificationService? notificationService = null)
        {
            this._friendRepository = friendRepository;
            this._userRepository = userRepository;
            this._conversationRepository = conversationRepository;
            this._participantRepository = participantRepository;
            this._notificationService = notificationService;
        }

        public async Task SendFriendRequestAsync(Guid senderUserId, Guid receiverUserId)
        {
            if (senderUserId == receiverUserId)
            {
                throw new InvalidOperationException("You cannot send a friend request to yourself.");
            }

            var areAlreadyFriends = await _friendRepository.CheckIfFriendsAsync(senderUserId, receiverUserId);
            if (areAlreadyFriends)
            {
                throw new InvalidOperationException("Users are already friends.");
            }

            var existingFriendshipRelation = await _friendRepository.GetFriendshipAsync(senderUserId, receiverUserId);
            if (existingFriendshipRelation != null)
            {
                if (existingFriendshipRelation.Status == FriendStatus.Blocked)
                {
                    await _friendRepository.UpdateFriendshipStatusAsync(senderUserId, receiverUserId, FriendStatus.Pending);
                    await _friendRepository.SetMatchStatusAsync(senderUserId, receiverUserId, false);
                    await TryNotifyFriendRequestAsync(senderUserId, receiverUserId);
                    return;
                }

                throw new InvalidOperationException("A friend request already exists between these users.");
            }

            var newFriendRequest = new Friend
            {
                Id = Guid.NewGuid(),
                UserId1 = senderUserId,
                UserId2 = receiverUserId,
                Status = FriendStatus.Pending,
                IsMatch = false,
                CreatedAt = DateTime.UtcNow
            };

            await _friendRepository.CreateFriendshipAsync(newFriendRequest);
            await TryNotifyFriendRequestAsync(senderUserId, receiverUserId);
        }

        private async Task TryNotifyFriendRequestAsync(Guid senderUserId, Guid receiverUserId)
        {
            if (_notificationService == null)
            {
                return;
            }

            try
            {
                var sender = await _userRepository.GetByIdAsync(senderUserId);
                var senderName = sender?.Username ?? "Someone";
                await _notificationService.NotifyAsync(
                    receiverUserId,
                    "New friend request",
                    $"{senderName} sent you a friend request.");
            }
            catch
            {
                // Notifications should not block the friend request itself.
            }
        }

        public async Task<bool> SendFriendRequestByUsernameAsync(Guid senderUserId, string receiverUsername)
        {
            if (string.IsNullOrWhiteSpace(receiverUsername))
            {
                throw new InvalidOperationException("Please enter a username.");
            }

            var receiverUserObject = await _userRepository.GetByUsernameAsync(receiverUsername.Trim());
            if (receiverUserObject == null)
            {
                return false;
            }

            await SendFriendRequestAsync(senderUserId, receiverUserObject.Id);
            return true;
        }

        public async Task AcceptFriendRequestAsync(Guid currentUserId, Guid requesterUserId)
        {
            await _friendRepository.UpdateFriendshipStatusAsync(requesterUserId, currentUserId, FriendStatus.Accepted);

            var existingConversation = await _conversationRepository.GetDmBetweenAsync(currentUserId, requesterUserId);
            if (existingConversation != null)
            {
                return;
            }

            var newConversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Dm,
                Title = null,
                IconUrl = null,
                CreatedBy = currentUserId,
                PinnedMessageId = null
            };

            await _conversationRepository.CreateAsync(newConversation);

            var currentDataTime = DateTime.UtcNow;

            await _participantRepository.CreateAsync(new Participant
            {
                Id = Guid.NewGuid(),
                ConversationId = newConversation.Id,
                UserId = currentUserId,
                JoinedAt = currentDataTime,
                Role = ParticipantRole.Member,
                LastReadMessageId = null,
                TimeoutUntil = null,
                IsFavourite = false
            });

            await _participantRepository.CreateAsync(new Participant
            {
                Id = Guid.NewGuid(),
                ConversationId = newConversation.Id,
                UserId = requesterUserId,
                JoinedAt = currentDataTime,
                Role = ParticipantRole.Member,
                LastReadMessageId = null,
                TimeoutUntil = null,
                IsFavourite = false
            });
        }

        public async Task DeclineFriendRequestAsync(Guid currentUserId, Guid requesterUserId)
        {
            var friendshipRelation = await _friendRepository.GetFriendshipAsync(requesterUserId, currentUserId);

            if (friendshipRelation == null || friendshipRelation.Status != FriendStatus.Pending)
            {
                throw new InvalidOperationException("No pending friend request found.");
            }

            await _friendRepository.DeleteFriendshipAsync(requesterUserId, currentUserId);
        }

        public async Task<List<User>> GetIncomingRequestsAsync(Guid currentUserId)
        {
            var pendingRequestList = await _friendRepository.GetPendingRequestsForUserAsync(currentUserId);
            var senderUserList = new List<User>();

            foreach (var requestRelation in pendingRequestList)
            {
                var senderUserObject = await _userRepository.GetByIdAsync(requestRelation.UserId1);
                if (senderUserObject != null)
                {
                    senderUserList.Add(senderUserObject);
                }
            }

            return senderUserList;
        }

        public async Task<FriendStatus?> GetRelationshipStatusAsync(Guid firstUserId, Guid secondUserId)
        {
            var friendshipRelation = await _friendRepository.GetFriendshipAsync(firstUserId, secondUserId);
            return friendshipRelation?.Status;
        }
    }
}
