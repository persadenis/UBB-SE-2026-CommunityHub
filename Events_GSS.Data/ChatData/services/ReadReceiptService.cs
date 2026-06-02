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
    public class ReadReceiptService : IReadReceiptService
    {
        private readonly IParticipantRepository _participantRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public ReadReceiptService(
            IParticipantRepository participantRepository,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            this._participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
            this._messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            this._userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task MarkAsReadAsync(Guid conversationId, Guid userId, Guid messageId)
        {
            var participant = await this._participantRepository.GetAsync(conversationId, userId);
            if (participant == null)
            {
                throw new InvalidOperationException("User is not a participant of this conversation.");
            }

            var targetMessage = await this._messageRepository.GetByIdAsync(messageId);
            if (targetMessage == null || targetMessage.ConversationId != conversationId)
            {
                return;
            }

            if (targetMessage.UserId.HasValue && targetMessage.UserId.Value == userId)
            {
                return;
            }

            if (participant.LastReadMessageId.HasValue)
            {
                var currentLastRead = await this._messageRepository.GetByIdAsync(participant.LastReadMessageId.Value);
                if (currentLastRead != null && currentLastRead.CreatedAt >= targetMessage.CreatedAt)
                {
                    return;
                }
            }

            await this._participantRepository.UpdateLastReadAsync(conversationId, userId, messageId);
        }

        public async Task MarkLatestAsReadAsync(Guid conversationId, Guid userId)
        {
            var latestReadableMessageId = await this._messageRepository.GetLatestReadableMessageIdAsync(conversationId, userId);
            if (!latestReadableMessageId.HasValue)
            {
                return;
            }

            await this.MarkAsReadAsync(conversationId, userId, latestReadableMessageId.Value);
        }

        public async Task<List<Participant>> GetReadReceiptsAsync(Guid conversationId, Guid messageId)
        {
            var targetMessage = await this._messageRepository.GetByIdAsync(messageId);
            if (targetMessage == null)
            {
                return new List<Participant>();
            }

            var participants = await this._participantRepository.GetAllForConversationAsync(conversationId);
            var readers = new List<Participant>();

            foreach (var participant in participants)
            {
                if (!participant.LastReadMessageId.HasValue)
                {
                    continue;
                }

                var lastRead = await this._messageRepository.GetByIdAsync(participant.LastReadMessageId.Value);
                if (lastRead != null && lastRead.CreatedAt >= targetMessage.CreatedAt)
                {
                    readers.Add(participant);
                }
            }

            return readers;
        }

        public async Task<int> GetReadByCountAsync(Guid conversationId, Guid messageId)
        {
            var readers = await this.GetReadReceiptsAsync(conversationId, messageId);
            return readers.Count;
        }

        public async Task<int> GetReadByOthersCountAsync(Guid conversationId, Guid messageId, Guid currentUserId)
        {
            var readers = await this.GetReadReceiptsAsync(conversationId, messageId);
            return readers.Count(participant => participant.UserId != currentUserId);
        }

        public async Task<Guid?> GetLastReadMessageAsync(Guid conversationId, Guid userId)
        {
            var participant = await this._participantRepository.GetAsync(conversationId, userId);
            return participant?.LastReadMessageId;
        }

        public async Task<List<Participant>> GetParticipantsAsync(Guid conversationId)
        {
            return await this._participantRepository.GetAllForConversationAsync(conversationId);
        }

        public async Task<DateTime?> GetLastReadTimestampAsync(Guid conversationId, Guid userId)
        {
            var participant = await this._participantRepository.GetAsync(conversationId, userId);
            if (participant?.LastReadMessageId == null)
            {
                return null;
            }

            var lastReadMessage = await this._messageRepository.GetByIdAsync(participant.LastReadMessageId.Value);
            return lastReadMessage?.CreatedAt;
        }

        public async Task<List<string>> GetReaderUsernamesAsync(Guid conversationId, Guid messageId, Guid? excludeUserId = null)
        {
            var readers = await this.GetReadReceiptsAsync(conversationId, messageId);
            var usernames = new List<string>();

            foreach (var reader in readers)
            {
                if (excludeUserId.HasValue && reader.UserId == excludeUserId.Value)
                {
                    continue;
                }

                var user = await this._userRepository.GetByIdAsync(reader.UserId);
                if (user != null)
                {
                    usernames.Add(user.Username);
                }
            }

            return usernames;
        }
    }
}