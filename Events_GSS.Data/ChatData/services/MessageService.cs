using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.EventsData.Services.notificationServices;

namespace ChatAndEvents.Data.ChatData.services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly INotificationService? _notificationService;

        public MessageService(
            IMessageRepository messageRepository,
            IParticipantRepository participantRepository,
            IUserRepository userRepository,
            IConversationRepository conversationRepository,
            INotificationService? notificationService = null)
        {
            _messageRepository = messageRepository;
            _participantRepository = participantRepository;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
            _notificationService = notificationService;
        }

        private async Task<Participant> RequireActiveParticipantAsync(Guid conversationId, Guid userId)
        {
            var participant = await _participantRepository.GetAsync(conversationId, userId);
            if (participant == null)
            {
                throw new InvalidOperationException("Participant not found for this conversation.");
            }

            if (participant.Role == ParticipantRole.Banned)
            {
                throw new InvalidOperationException("Participant is banned in this conversation.");
            }

            return participant;
        }

        private async Task RequireCanSendAsync(Guid conversationId, Guid userId)
        {
            var participant = await RequireActiveParticipantAsync(conversationId, userId);
            if (participant.TimeoutUntil.HasValue && participant.TimeoutUntil.Value > DateTime.UtcNow)
            {
                var remaining = participant.TimeoutUntil.Value - DateTime.UtcNow;
                if (remaining < TimeSpan.Zero)
                {
                    remaining = TimeSpan.Zero;
                }

                throw new InvalidOperationException($"You are timed out and cannot send messages for {FormatDuration(remaining)}.");
            }
        }

        public async Task<string?> GetCannotSendReasonAsync(Guid conversationId, Guid userId)
        {
            var participant = await _participantRepository.GetAsync(conversationId, userId);
            if (participant == null)
            {
                return "You are not a participant of this conversation.";
            }

            if (participant.Role == ParticipantRole.Banned)
            {
                return "You are banned in this conversation.";
            }

            if (participant.TimeoutUntil.HasValue && participant.TimeoutUntil.Value > DateTime.UtcNow)
            {
                var remaining = participant.TimeoutUntil.Value - DateTime.UtcNow;
                if (remaining < TimeSpan.Zero)
                {
                    remaining = TimeSpan.Zero;
                }

                return $"You are timed out and cannot send messages for {FormatDuration(remaining)}.";
            }

            return null;
        }

        private static string FormatDuration(TimeSpan duration)
        {
            var totalSeconds = Math.Max(0, (int)Math.Ceiling(duration.TotalSeconds));
            var days = totalSeconds / 86400;
            totalSeconds %= 86400;
            var hours = totalSeconds / 3600;
            totalSeconds %= 3600;
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;

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

            if (minutes > 0)
            {
                return seconds > 0
                    ? (minutes == 1 ? $"1 minute {seconds} seconds" : $"{minutes} minutes {seconds} seconds")
                    : (minutes == 1 ? "1 minute" : $"{minutes} minutes");
            }

            return seconds <= 1 ? "1 second" : $"{seconds} seconds";
        }

        public async Task<List<Message>> GetMessagesAsync(Guid conversationId, Guid userId, int skip, int take)
        {
            await RequireActiveParticipantAsync(conversationId, userId);

            var messages = await _messageRepository.GetByConversationAsync(conversationId, skip, take);
            await PopulateSenderMetadataAsync(messages);
            return messages;
        }

        public async Task<Message> SendMessageAsync(Guid conversationId, Guid senderId, string content, Guid? replyToId)
        {
            await RequireCanSendAsync(conversationId, senderId);

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Message content cannot be empty.", nameof(content));
            }

            if (content.Length > 1024)
            {
                throw new InvalidOperationException("Message length cannot exceed 1024 characters.");
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserId = senderId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ReplyToId = replyToId,
                IsEdited = false,
                IsDeleted = false,
                MessageType = MessageType.Text,
                ParentMessageId = null
            };

            await _messageRepository.CreateAsync(message);

            var sender = await _userRepository.GetByIdAsync(senderId);
            message.SenderUsername = sender?.Username;
            message.SenderAvatarUrl = sender?.AvatarUrl;

            try
            {
                await NotifyConversationParticipantsAsync(conversationId, senderId, sender?.Username, content);
            }
            catch
            {
                // Notifications should never prevent the message itself from being sent.
            }

            return message;
        }

        private async Task NotifyConversationParticipantsAsync(
            Guid conversationId,
            Guid senderId,
            string? senderUsername,
            string content)
        {
            if (_notificationService == null)
            {
                return;
            }

            var participants = await _participantRepository.GetAllForConversationAsync(conversationId);
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            var senderName = string.IsNullOrWhiteSpace(senderUsername) ? "Someone" : senderUsername;
            var title = conversation?.Type == ConversationType.Group && !string.IsNullOrWhiteSpace(conversation.Title)
                ? $"New message in {conversation.Title}"
                : "New message";
            var preview = content.Length > 80 ? $"{content[..80]}..." : content;
            var description = $"{senderName}: {preview}";

            foreach (var participant in participants.Where(participant => participant.UserId != senderId))
            {
                await _notificationService.NotifyAsync(participant.UserId, title, description);
            }
        }

        public Task<string> PersistImageAttachmentAsync(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                throw new InvalidOperationException("Attachment file was not found.");
            }

            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
            {
                throw new InvalidOperationException("Only PNG and JPEG images are supported.");
            }

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var attachmentsDir = Path.Combine(appData, "ChatModule", "attachments");
            Directory.CreateDirectory(attachmentsDir);

            var appFolder = AppContext.BaseDirectory;
            var binAttachmentsDir = Path.Combine(appFolder, "attachments");
            Directory.CreateDirectory(binAttachmentsDir);

            var targetFileName = $"{Guid.NewGuid():N}{extension}";
            var targetPath = Path.Combine(attachmentsDir, targetFileName);
            File.Copy(sourcePath, targetPath, overwrite: false);

            var binTargetPath = Path.Combine(binAttachmentsDir, targetFileName);
            if (!File.Exists(binTargetPath))
            {
                File.Copy(sourcePath, binTargetPath, overwrite: false);
            }

            return Task.FromResult(targetPath);
        }

        public async Task EditMessageAsync(Guid messageId, Guid requesterId, string newContent)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                throw new InvalidOperationException("Message not found.");

            if (message.UserId == null || message.UserId.Value != requesterId)
                throw new UnauthorizedAccessException("You are not the author of this message.");

            await _messageRepository.UpdateContentAsync(messageId, newContent);
            await _messageRepository.SetEditedAsync(messageId);
        }

        public async Task DeleteMessageAsync(Guid messageId, Guid requesterId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
                throw new InvalidOperationException("Message not found.");

            bool isAuthor = message.UserId != null && message.UserId.Value == requesterId;
            bool isAdmin = false;

            if (!isAuthor)
            {
                var participant = await _participantRepository.GetAsync(message.ConversationId, requesterId);
                isAdmin = participant != null && participant.Role == ParticipantRole.Admin;
            }

            if (!isAuthor && !isAdmin)
                throw new UnauthorizedAccessException("You do not have permission to delete this message.");

            await _messageRepository.SoftDeleteAsync(messageId);
        }

        private async Task PopulateSenderMetadataAsync(List<Message> messages)
        {
            foreach (var message in messages)
            {
                if (!message.UserId.HasValue)
                {
                    message.SenderUsername = "System";
                    message.SenderAvatarUrl = null;
                    continue;
                }

                var sender = await _userRepository.GetByIdAsync(message.UserId.Value);
                var displayName = sender?.Username ?? "Unknown";

                var participant = await _participantRepository.GetAsync(message.ConversationId, message.UserId.Value);
                var conversation = await _conversationRepository.GetByIdAsync(message.ConversationId);
                if (conversation?.Type == ConversationType.Group && !string.IsNullOrWhiteSpace(participant?.Nickname))
                {
                    displayName = participant.Nickname!;
                }

                message.SenderUsername = displayName;
                message.SenderAvatarUrl = sender?.AvatarUrl;
            }
        }

        public async Task SetNicknameAsync(Guid conversationId, Guid userId, string? nickname)
        {
            if (!string.IsNullOrWhiteSpace(nickname) && nickname.Length > 16)
            {
                throw new InvalidOperationException("Nickname cannot exceed 16 characters.");
            }

            await _participantRepository.UpdateNicknameAsync(conversationId, userId, string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim());
        }
    }
}
