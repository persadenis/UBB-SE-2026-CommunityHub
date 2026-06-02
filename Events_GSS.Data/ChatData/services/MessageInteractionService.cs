using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class MessageInteractionService : IMessageInteractionService
    {
        private const int MaxPreviewLength = 100;

        private readonly IMessageRepository _messageRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IUserRepository _userRepository;

        public MessageInteractionService(
            IMessageRepository messageRepository,
            IParticipantRepository participantRepository,
            IUserRepository userRepository)
        {
            this._messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            this._participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
            this._userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task ReactToMessageAsync(Guid messageId, Guid userId, string emoji)
        {
            var message = await this._messageRepository.GetByIdAsync(messageId)
                ?? throw new InvalidOperationException("Message not found.");

            if (string.IsNullOrWhiteSpace(emoji))
            {
                throw new InvalidOperationException("Reaction cannot be empty.");
            }

            if (message.MessageType == MessageType.Reaction)
            {
                throw new InvalidOperationException("You cannot react to a reaction message.");
            }

            await this.RequireCanSendAsync(message.ConversationId, userId);

            var existingReactions = await this._messageRepository.GetReactionsForMessageAsync(messageId);
            var existingActive = existingReactions.FirstOrDefault(r => r.UserId == userId && !r.IsDeleted);
            var existingDeleted = existingReactions.FirstOrDefault(r => r.UserId == userId && r.IsDeleted);

            if (existingActive != null)
            {
                await this._messageRepository.UpdateContentAsync(existingActive.Id, emoji);
            }
            else if (existingDeleted != null)
            {
                await this._messageRepository.UpdateContentAsync(existingDeleted.Id, emoji);
                await this._messageRepository.UnsoftDeleteAsync(existingDeleted.Id);
            }
            else
            {
                await this._messageRepository.CreateAsync(new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = message.ConversationId,
                    UserId = userId,
                    Content = emoji,
                    CreatedAt = DateTime.UtcNow,
                    ReplyToId = null,
                    IsEdited = false,
                    IsDeleted = false,
                    MessageType = MessageType.Reaction,
                    ParentMessageId = messageId
                });
            }
        }

        public async Task RemoveReactionAsync(Guid messageId, Guid userId)
        {
            var message = await this._messageRepository.GetByIdAsync(messageId)
                ?? throw new InvalidOperationException("Message not found.");

            await this.RequireCanSendAsync(message.ConversationId, userId);

            var existingReactions = await this._messageRepository.GetReactionsForMessageAsync(messageId);
            var existing = existingReactions.FirstOrDefault(r => r.UserId == userId && !r.IsDeleted);

            if (existing != null)
            {
                await this._messageRepository.SoftDeleteAsync(existing.Id);
            }
            else
            {
                throw new InvalidOperationException("Reaction not found for this user.");
            }
        }

        public async Task<List<Message>> GetReactionsAsync(Guid messageId)
        {
            var message = await this._messageRepository.GetByIdAsync(messageId)
                ?? throw new InvalidOperationException("Message not found.");

            return await this._messageRepository.GetReactionsForMessageAsync(messageId);
        }

        public async Task<string?> BuildReplyPreviewAsync(Guid messageId)
        {
            var message = await this._messageRepository.GetByIdAsync(messageId)
                ?? throw new InvalidOperationException("Message not found.");

            if (message.IsDeleted)
            {
                return "This message has been deleted.";
            }

            if (message.MessageType == MessageType.Reaction)
            {
                return "This is a reaction and cannot be previewed.";
            }

            var senderName = "Unknown User";
            if (message.UserId.HasValue)
            {
                var user = await this._userRepository.GetByIdAsync(message.UserId.Value);
                if (user != null)
                {
                    senderName = user.Username;
                }
            }

            var contentPreview = message.Content != null
                ? (message.Content.Length > MaxPreviewLength ? message.Content.Substring(0, MaxPreviewLength) + "..." : message.Content)
                : "[No Text]";

            return $"{senderName}: {contentPreview}";
        }

        public async Task<(string Sender, string Content)?> BuildReplyPreviewPartsAsync(Guid messageId)
        {
            var message = await this._messageRepository.GetByIdAsync(messageId)
                ?? throw new InvalidOperationException("Message not found.");

            if (message.IsDeleted)
            {
                return ("Deleted", "This message has been deleted.");
            }

            if (message.MessageType == MessageType.Reaction)
            {
                return ("Reaction", "This is a reaction and cannot be previewed.");
            }

            var senderName = "Unknown User";
            if (message.UserId.HasValue)
            {
                var user = await this._userRepository.GetByIdAsync(message.UserId.Value);
                if (user != null)
                {
                    senderName = user.Username;
                }
            }

            var contentPreview = message.Content != null
                ? (message.Content.Length > MaxPreviewLength ? message.Content.Substring(0, MaxPreviewLength) + "..." : message.Content)
                : "[No Text]";

            return (senderName, contentPreview);
        }

        private async Task<Participant> RequireActiveParticipantAsync(Guid conversationId, Guid userId)
        {
            var participant = await this._participantRepository.GetAsync(conversationId, userId);
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
            var participant = await this.RequireActiveParticipantAsync(conversationId, userId);
            if (participant.TimeoutUntil.HasValue && participant.TimeoutUntil.Value > DateTime.UtcNow)
            {
                throw new InvalidOperationException("Participant is timed out and cannot send messages.");
            }
        }
    }
}