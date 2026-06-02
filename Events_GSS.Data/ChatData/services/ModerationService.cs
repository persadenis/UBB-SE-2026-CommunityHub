using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class ModerationService : IModerationService
    {
        private readonly IParticipantRepository _participantRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public ModerationService(
            IParticipantRepository participantRepository,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
        
        private async Task<Participant> RequireAdminAsync(Guid conversationId, Guid userId)
        {
            var participant = await _participantRepository.GetAsync(conversationId, userId);
            if (participant == null || participant.Role != ParticipantRole.Admin)
            {
                throw new InvalidOperationException("Only admins can perform this action.");
            }

            return participant;
        }

        private async Task<Participant> RequireTargetParticipantAsync(Guid conversationId, Guid userId)
        {
            var participant = await _participantRepository.GetAsync(conversationId, userId);
            if (participant == null)
            {
                throw new InvalidOperationException("Target user is not part of this conversation.");
            }

            return participant;
        }

        private async Task EnsureUserExistsAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }
        }

        public async Task BanMemberAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            await RequireAdminAsync(conversationId, adminId);
            await RequireTargetParticipantAsync(conversationId, targetId);

            await _participantRepository.UpdateRoleAsync(conversationId, targetId, ParticipantRole.Banned);

            var username = await ResolveUsernameAsync(targetId);
            await WriteSystemMessageAsync(conversationId, $"{username} was banned.");
        }

        public async Task UnbanMemberAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            await RequireAdminAsync(conversationId, adminId);
            await RequireTargetParticipantAsync(conversationId, targetId);

            await _participantRepository.UpdateRoleAsync(conversationId, targetId, ParticipantRole.Member);

            var username = await ResolveUsernameAsync(targetId);
            await WriteSystemMessageAsync(conversationId, $"{username} was unbanned.");
        }

        public async Task TimeoutMemberAsync(Guid conversationId, Guid adminId, Guid targetId, TimeSpan duration)
        {
            await RequireAdminAsync(conversationId, adminId);
            await RequireTargetParticipantAsync(conversationId, targetId);

            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentException("Timeout duration must be greater than zero.", nameof(duration));
            }

            await _participantRepository.UpdateTimeoutAsync(conversationId, targetId, DateTime.UtcNow + duration);

            var username = await ResolveUsernameAsync(targetId);
            await WriteSystemMessageAsync(conversationId, $"{username} was timed out for {FormatDuration(duration)}.");
        }

        public async Task RemoveTimeoutAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            await RequireAdminAsync(conversationId, adminId);
            await _participantRepository.UpdateTimeoutAsync(conversationId, targetId, null);
            var username = await ResolveUsernameAsync(targetId);
            await WriteSystemMessageAsync(conversationId, $"Timeout removed for {username}.");
        }

        public async Task PromoteMemberAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            await RequireAdminAsync(conversationId, adminId);
            await _participantRepository.UpdateRoleAsync(conversationId, targetId, ParticipantRole.Admin);
            var username = await ResolveUsernameAsync(targetId);
            await WriteSystemMessageAsync(conversationId, $"{username} was promoted to admin.");
        }

        public async Task DemoteMemberAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            await RequireAdminAsync(conversationId, adminId);

            if (adminId == targetId)
            {
                var participants = await _participantRepository.GetAllForConversationAsync(conversationId);
                var adminCount = 0;
                foreach (var participant in participants)
                {
                    if (participant.Role == ParticipantRole.Admin)
                    {
                        adminCount++;
                    }
                }

                if (adminCount <= 1)
                {
                    throw new InvalidOperationException("You cannot demote the only admin in the group.");
                }
            }

            await _participantRepository.UpdateRoleAsync(conversationId, targetId, ParticipantRole.Member);
            var username = await ResolveUsernameAsync(targetId);
            await WriteSystemMessageAsync(conversationId, $"{username} was demoted to member.");
        }

        public async Task AddMemberAsync(Guid conversationId, Guid adminId, Guid newUserId)
        {
            await RequireAdminAsync(conversationId, adminId);
            await EnsureUserExistsAsync(newUserId);

            var existingParticipant = await _participantRepository.GetAsync(conversationId, newUserId);
            if (existingParticipant != null)
            {
                throw new InvalidOperationException("User is already a participant of this conversation.");
            }

            await _participantRepository.CreateAsync(new Participant
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserId = newUserId,
                JoinedAt = DateTime.UtcNow,
                Role = ParticipantRole.Member,
                LastReadMessageId = null,
                TimeoutUntil = null,
                IsFavourite = false,
                IsNew = false,
                Nickname = null
            });

            var username = await ResolveUsernameAsync(newUserId);
            await WriteSystemMessageAsync(conversationId, $"{username} was added to the group.");
        }

        private async Task<string> ResolveUsernameAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Username ?? userId.ToString();
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

        private static string FormatDuration(TimeSpan duration)
        {
            var totalSeconds = Math.Max(0, (int)Math.Round(duration.TotalSeconds));
            var days = totalSeconds / 86400;
            totalSeconds %= 86400;
            var hours = totalSeconds / 3600;
            totalSeconds %= 3600;
            var minutes = totalSeconds / 60;

            if (days > 0)
            {
                return days == 1 ? "1 day" : $"{days} days";
            }

            if (hours > 0)
            {
                return minutes > 0
                    ? (hours == 1 ? $"1 hour {minutes} minutes" : $"{hours} hours {minutes} minutes")
                    : (hours == 1 ? "1 hour" : $"{hours} hours");
            }

            return minutes == 1 ? "1 minute" : $"{minutes} minutes";
        }
    }
}
