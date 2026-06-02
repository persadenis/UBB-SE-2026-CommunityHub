using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;
using Moq;
using Xunit;

namespace ChatModule.Tests.Services
{
    public class MessageServiceTests
    {
        private readonly Mock<IMessageRepository> _mockMessageRepository;
        private readonly Mock<IParticipantRepository> _mockParticipantRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IConversationRepository> _mockConversationRepository;
        private readonly MessageService _messageService;

        public MessageServiceTests()
        {
            this._mockMessageRepository = new Mock<IMessageRepository>();
            this._mockParticipantRepository = new Mock<IParticipantRepository>();
            this._mockUserRepository = new Mock<IUserRepository>();
            this._mockConversationRepository = new Mock<IConversationRepository>();

            this._messageService = new MessageService(
                this._mockMessageRepository.Object,
                this._mockParticipantRepository.Object,
                this._mockUserRepository.Object,
                this._mockConversationRepository.Object);
        }

        [Fact]
        public async Task SendMessageAsync_EmptyContent_ThrowsArgumentException()
        {
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var emptyContent = string.Empty;

            var participant = new Participant { Role = ParticipantRole.Member };
            this._mockParticipantRepository
                .Setup(repo => repo.GetAsync(conversationId, senderId))
                .ReturnsAsync(participant);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                this._messageService.SendMessageAsync(conversationId, senderId, emptyContent, null));
        }

        [Fact]
        public async Task SendMessageAsync_ContentExceedsLimit_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var longContent = new string('A', 1025);

            var participant = new Participant { Role = ParticipantRole.Member };
            this._mockParticipantRepository
                .Setup(repo => repo.GetAsync(conversationId, senderId))
                .ReturnsAsync(participant);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._messageService.SendMessageAsync(conversationId, senderId, longContent, null));
        }

        [Fact]
        public async Task SendMessageAsync_ValidData_CreatesMessage()
        {
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var validContent = "Hello, world!";

            var participant = new Participant { Role = ParticipantRole.Member };
            this._mockParticipantRepository
                .Setup(repo => repo.GetAsync(conversationId, senderId))
                .ReturnsAsync(participant);

            var senderUser = new User { Id = senderId, Username = "TestUser", AvatarUrl = "url" };
            this._mockUserRepository
                .Setup(repo => repo.GetByIdAsync(senderId))
                .ReturnsAsync(senderUser);

            await this._messageService.SendMessageAsync(conversationId, senderId, validContent, null);

            this._mockMessageRepository.Verify(
                repo => repo.CreateAsync(It.Is<Message>(m => m.Content == validContent && m.ConversationId == conversationId)),
                Times.Once);
        }

        [Fact]
        public async Task SetNicknameAsync_ValidNickname_UpdatesRepository()
        {
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var validNickname = "SuperAdmin";

            await this._messageService.SetNicknameAsync(conversationId, userId, validNickname);

            this._mockParticipantRepository.Verify(
                repo => repo.UpdateNicknameAsync(conversationId, userId, validNickname),
                Times.Once);
        }
    }
}