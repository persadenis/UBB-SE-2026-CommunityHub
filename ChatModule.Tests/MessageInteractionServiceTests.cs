using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ChatModule.Tests.Services
{
    public class MessageInteractionServiceTests
    {
        private readonly Mock<IMessageRepository> _mockMessageRepository;
        private readonly Mock<IParticipantRepository> _mockParticipantRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly MessageInteractionService _interactionService;

        public MessageInteractionServiceTests()
        {
            this._mockMessageRepository = new Mock<IMessageRepository>();
            this._mockParticipantRepository = new Mock<IParticipantRepository>();
            this._mockUserRepository = new Mock<IUserRepository>();

            this._interactionService = new MessageInteractionService(
                this._mockMessageRepository.Object,
                this._mockParticipantRepository.Object,
                this._mockUserRepository.Object);
        }

        [Fact]
        public async Task ReactToMessageAsync_MessageNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            this._mockMessageRepository
                .Setup(repo => repo.GetByIdAsync(messageId))
                .ReturnsAsync((Message?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._interactionService.ReactToMessageAsync(messageId, userId, "👍"));
        }

        [Fact]
        public async Task ReactToMessageAsync_EmptyEmoji_ThrowsInvalidOperationException()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var validMessage = new Message { Id = messageId, MessageType = MessageType.Text };

            this._mockMessageRepository
                .Setup(repo => repo.GetByIdAsync(messageId))
                .ReturnsAsync(validMessage);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._interactionService.ReactToMessageAsync(messageId, userId, string.Empty));
        }

        [Fact]
        public async Task ReactToMessageAsync_ValidNewReaction_CreatesMessage()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var emoji = "🔥";

            var validMessage = new Message { Id = messageId, ConversationId = conversationId, MessageType = MessageType.Text };
            var participant = new Participant { Role = ParticipantRole.Member };

            this._mockMessageRepository
                .Setup(repo => repo.GetByIdAsync(messageId))
                .ReturnsAsync(validMessage);

            this._mockParticipantRepository
                .Setup(repo => repo.GetAsync(conversationId, userId))
                .ReturnsAsync(participant);

            this._mockMessageRepository
                .Setup(repo => repo.GetReactionsForMessageAsync(messageId))
                .ReturnsAsync(new List<Message>());

            await this._interactionService.ReactToMessageAsync(messageId, userId, emoji);

            this._mockMessageRepository.Verify(
                repo => repo.CreateAsync(It.Is<Message>(m => m.Content == emoji && m.MessageType == MessageType.Reaction)),
                Times.Once);
        }

        [Fact]
        public async Task RemoveReactionAsync_ReactionExists_SoftDeletesReaction()
        {
            var conversationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reactionId = Guid.NewGuid();

            var validMessage = new Message { Id = messageId, ConversationId = conversationId };
            var participant = new Participant { Role = ParticipantRole.Member };
            var existingReaction = new Message { Id = reactionId, UserId = userId, IsDeleted = false };

            this._mockMessageRepository
                .Setup(repo => repo.GetByIdAsync(messageId))
                .ReturnsAsync(validMessage);

            this._mockParticipantRepository
                .Setup(repo => repo.GetAsync(conversationId, userId))
                .ReturnsAsync(participant);

            this._mockMessageRepository
                .Setup(repo => repo.GetReactionsForMessageAsync(messageId))
                .ReturnsAsync(new List<Message> { existingReaction });

            // Act
            await this._interactionService.RemoveReactionAsync(messageId, userId);

            this._mockMessageRepository.Verify(
                repo => repo.SoftDeleteAsync(reactionId),
                Times.Once);
        }

        [Fact]
        public async Task BuildReplyPreviewAsync_MessageExceedsLimit_ReturnsTruncatedString()
        {
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var longContent = new string('A', 150);
            var validMessage = new Message
            {
                Id = messageId,
                UserId = userId,
                Content = longContent,
                MessageType = MessageType.Text,
                IsDeleted = false
            };

            var user = new User { Id = userId, Username = "JohnDoe" };

            this._mockMessageRepository
                .Setup(repo => repo.GetByIdAsync(messageId))
                .ReturnsAsync(validMessage);

            this._mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var preview = await this._interactionService.BuildReplyPreviewAsync(messageId);

            // Assert
            Assert.Equal("JohnDoe: " + new string('A', 100) + "...", preview);
        }
    }
}