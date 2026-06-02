using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;

namespace ChatAndEvents.Data.ChatData.services
{
    public class GroupService : IGroupService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public GroupService(
            IConversationRepository conversationRepository,
            IParticipantRepository participantRepository,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
            _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<Conversation> CreateGroupAsync(Guid creatorId, string title, string? iconUrl, List<Guid> memberIds)
        {
            var sanitizedTitle = title?.Trim();
            if (string.IsNullOrWhiteSpace(sanitizedTitle))
            {
                throw new ArgumentException("Group title cannot be empty.", nameof(title));
            }

            var uniqueMemberIds = (memberIds ?? new List<Guid>())
                .Where(memberId => memberId != Guid.Empty)
                .Distinct()
                .Where(memberId => memberId != creatorId)
                .ToList();

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Group,
                Title = sanitizedTitle,
                IconUrl = iconUrl,
                CreatedBy = creatorId,
                PinnedMessageId = null
            };

            await _conversationRepository.CreateAsync(conversation);

            var joinedAt = DateTime.UtcNow;

            await _participantRepository.CreateAsync(new Participant
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                UserId = creatorId,
                JoinedAt = joinedAt,
                Role = ParticipantRole.Admin,
                LastReadMessageId = null,
                TimeoutUntil = null,
                IsFavourite = false,
                IsNew = false,
                Nickname = null
            });

            foreach (var memberId in uniqueMemberIds)
            {
                await _participantRepository.CreateAsync(new Participant
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    UserId = memberId,
                    JoinedAt = joinedAt,
                    Role = ParticipantRole.Member,
                    LastReadMessageId = null,
                    TimeoutUntil = null,
                    IsFavourite = false,
                    IsNew = false,
                    Nickname = null
                });
            }

            var creatorName = await ResolveUsernameAsync(creatorId);
            await WriteSystemMessageAsync(conversation.Id, $"Group \"{sanitizedTitle}\" was created by {creatorName}.");

            return conversation;
        }

        public async Task UpdateGroupInfoAsync(Guid conversationId, Guid requesterId, string? newTitle, string? newIconUrl)
        {
            await RequireAdminAsync(conversationId, requesterId);

            var conversation = await _conversationRepository.GetByIdAsync(conversationId)
                ?? throw new InvalidOperationException("Conversation not found.");

            if (newTitle != null)
                conversation.Title = newTitle;
            if (newIconUrl != null)
                conversation.IconUrl = newIconUrl;

            await _conversationRepository.UpdateAsync(conversation);
        }

        public async Task LeaveGroupAsync(Guid conversationId, Guid userId)
        {
            var leavingParticipant = await _participantRepository.GetAsync(conversationId, userId)
                ?? throw new InvalidOperationException("You are not a member of this conversation.");

            await _participantRepository.DeleteAsync(conversationId, userId);

            var remainingParticipants = await _participantRepository.GetAllForConversationAsync(conversationId);

            if (remainingParticipants.Count == 0)
            {
                await _messageRepository.DeleteByConversationAsync(conversationId);
                await _conversationRepository.DeleteAsync(conversationId);
                return;
            }

            var isLeavingAdmin = leavingParticipant.Role == ParticipantRole.Admin;
            var hasRemainingAdmin = remainingParticipants.Any(participant => participant.Role == ParticipantRole.Admin);
            var shouldPromoteOldestParticipant = isLeavingAdmin && !hasRemainingAdmin;

            if (shouldPromoteOldestParticipant)
            {
                var promotedParticipant = remainingParticipants
                    .OrderBy(participant => participant.JoinedAt)
                    .ThenBy(participant => participant.UserId)
                    .First();

                await _participantRepository.UpdateRoleAsync(conversationId, promotedParticipant.UserId, ParticipantRole.Admin);
                var promotedName = await ResolveUsernameAsync(promotedParticipant.UserId);
                await WriteSystemMessageAsync(conversationId, $"{promotedName} is now an admin.");
            }

            var leavingName = await ResolveUsernameAsync(userId);
            await WriteSystemMessageAsync(conversationId, $"{leavingName} left the group.");
        }

        public async Task PinMessageAsync(Guid conversationId, Guid requesterId, Guid messageId)
        {
            await RequireAdminAsync(conversationId, requesterId);

            var message = await _messageRepository.GetByIdAsync(messageId)
                ?? throw new InvalidOperationException("Message not found.");

            if (message.ConversationId != conversationId)
                throw new InvalidOperationException("Message does not belong to this conversation.");

            await _conversationRepository.SetPinnedMessageAsync(conversationId, messageId);
        }

        public async Task UnpinMessageAsync(Guid conversationId, Guid requesterId)
        {
            await RequireAdminAsync(conversationId, requesterId);
            await _conversationRepository.SetPinnedMessageAsync(conversationId, null);
        }

        public async Task PostEventNoticeAsync(Guid conversationId, Guid adminId, string eventTitle, DateTime eventDate)
        {
            await RequireAdminAsync(conversationId, adminId);
            await WriteSystemMessageAsync(conversationId, $"Event: \"{eventTitle}\" on {eventDate:f}.");
        }

        private async Task RequireAdminAsync(Guid conversationId, Guid userId)
        {
            var participant = await _participantRepository.GetAsync(conversationId, userId);
            if (participant == null)
                throw new InvalidOperationException("You are not a member of this conversation.");
            if (participant.Role != ParticipantRole.Admin)
                throw new UnauthorizedAccessException("Only admins can perform this action.");
        }

        private async Task WriteSystemMessageAsync(Guid conversationId, string text)
        {
            await _messageRepository.CreateAsync(new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserId = null,
                Content = text,
                CreatedAt = DateTime.UtcNow,
                ReplyToId = null,
                IsEdited = false,
                IsDeleted = false,
                MessageType = MessageType.System,
                ParentMessageId = null
            });
        }

        private async Task<string> ResolveUsernameAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Username ?? userId.ToString();
        }
    }
}
