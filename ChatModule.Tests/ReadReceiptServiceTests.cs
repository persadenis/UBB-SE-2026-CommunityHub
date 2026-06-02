using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ChatModule.Tests.Services
{
    public class ReadReceiptServiceTests
    {
        private readonly Mock<IParticipantRepository> _mockParticipantRepository;
        private readonly Mock<IMessageRepository> _mockMessageRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly ReadReceiptService _readReceiptService;

        public ReadReceiptServiceTests()
        {
            this._mockParticipantRepository = new Mock<IParticipantRepository>();
            this._mockMessageRepository = new Mock<IMessageRepository>();
            this._mockUserRepository = new Mock<IUserRepository>();

            this._readReceiptService = new ReadReceiptService(
                this._mockParticipantRepository.Object,
                this._mockMessageRepository.Object,
                this._mockUserRepository.Object);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotAParticipant_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            this._mockParticipantRepository.Setup(repo => repo.GetAsync(conversationId, userId)).ReturnsAsync((Participant?)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._readReceiptService.MarkAsReadAsync(conversationId, userId, messageId));

            Assert.Contains("not a participant", exception.Message);
        }

        [Fact]
        public async Task MarkAsReadAsync_ValidData_UpdatesLastRead()
        {
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var participant = new Participant { ConversationId = conversationId, UserId = userId, LastReadMessageId = null };
            var message = new Message { Id = messageId, ConversationId = conversationId, UserId = otherUserId, CreatedAt = DateTime.UtcNow };

            this._mockParticipantRepository.Setup(repo => repo.GetAsync(conversationId, userId)).ReturnsAsync(participant);
            this._mockMessageRepository.Setup(repo => repo.GetByIdAsync(messageId)).ReturnsAsync(message);

            await this._readReceiptService.MarkAsReadAsync(conversationId, userId, messageId);

            this._mockParticipantRepository.Verify(repo => repo.UpdateLastReadAsync(conversationId, userId, messageId), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_MessageBelongsToCurrentUser_ReturnsEarly()
        {
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var messageId = Guid.NewGuid();

            var participant = new Participant { ConversationId = conversationId, UserId = userId };
            var message = new Message { Id = messageId, ConversationId = conversationId, UserId = userId };

            this._mockParticipantRepository.Setup(repo => repo.GetAsync(conversationId, userId)).ReturnsAsync(participant);
            this._mockMessageRepository.Setup(repo => repo.GetByIdAsync(messageId)).ReturnsAsync(message);

            await this._readReceiptService.MarkAsReadAsync(conversationId, userId, messageId);

            this._mockParticipantRepository.Verify(repo => repo.UpdateLastReadAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetReadReceiptsAsync_ValidMessage_ReturnsReadersList()
        {
            var conversationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var targetMessage = new Message { Id = messageId, CreatedAt = DateTime.UtcNow.AddMinutes(-10) };

            var lastReadMessageId = Guid.NewGuid();
            var lastReadMessage = new Message { Id = lastReadMessageId, CreatedAt = DateTime.UtcNow }; // Newer message

            var readerParticipant = new Participant { UserId = Guid.NewGuid(), LastReadMessageId = lastReadMessageId };
            var unreadParticipant = new Participant { UserId = Guid.NewGuid(), LastReadMessageId = null };

            this._mockMessageRepository.Setup(repo => repo.GetByIdAsync(messageId)).ReturnsAsync(targetMessage);
            this._mockMessageRepository.Setup(repo => repo.GetByIdAsync(lastReadMessageId)).ReturnsAsync(lastReadMessage);
            this._mockParticipantRepository.Setup(repo => repo.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant> { readerParticipant, unreadParticipant });

            var readers = await this._readReceiptService.GetReadReceiptsAsync(conversationId, messageId);

            Assert.Single(readers);
            Assert.Equal(readerParticipant.UserId, readers[0].UserId);
        }

        [Fact]
        public async Task GetReaderUsernamesAsync_ValidData_ReturnsMappedUsernames()
        {
            var conversationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var readerUserId = Guid.NewGuid();
            var excludeUserId = Guid.NewGuid();

            var targetMessage = new Message { Id = messageId, CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
            var lastReadMsgId = Guid.NewGuid();
            var lastReadMsg = new Message { Id = lastReadMsgId, CreatedAt = DateTime.UtcNow };

            var reader1 = new Participant { UserId = readerUserId, LastReadMessageId = lastReadMsgId };
            var reader2ToExclude = new Participant { UserId = excludeUserId, LastReadMessageId = lastReadMsgId };

            this._mockMessageRepository.Setup(repo => repo.GetByIdAsync(messageId)).ReturnsAsync(targetMessage);
            this._mockMessageRepository.Setup(repo => repo.GetByIdAsync(lastReadMsgId)).ReturnsAsync(lastReadMsg);

            this._mockParticipantRepository.Setup(repo => repo.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant> { reader1, reader2ToExclude });

            this._mockUserRepository.Setup(repo => repo.GetByIdAsync(readerUserId))
                .ReturnsAsync(new User { Id = readerUserId, Username = "TestReader" });

            var usernames = await this._readReceiptService.GetReaderUsernamesAsync(conversationId, messageId, excludeUserId);

            Assert.Single(usernames);
            Assert.Equal("TestReader", usernames[0]);
        }
    }
}