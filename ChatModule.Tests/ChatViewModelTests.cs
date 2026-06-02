using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.src.view_models;
using Moq;
using Xunit;

namespace ChatModule.Tests.ViewModels
{
    public class ChatViewModelTests
    {
        private readonly Mock<IMessageService> _mockMessageService;
        private readonly Mock<IMessageInteractionService> _mockInteractionService;
        private readonly Mock<IReadReceiptService> _mockReadReceiptService;
        private readonly Mock<IMentionService> _mockMentionService;
        private readonly Mock<IDirectMessageService> _mockDirectMessageService;
        private readonly Mock<IConversationRepository> _mockConversationRepository;
        private readonly Mock<ISearchService> _mockSearchService;
        private readonly Guid _currentUserId;
        private readonly ChatViewModel _viewModel;

        public ChatViewModelTests()
        {
            this._mockMessageService = new Mock<IMessageService>();
            this._mockInteractionService = new Mock<IMessageInteractionService>();
            this._mockReadReceiptService = new Mock<IReadReceiptService>();
            this._mockMentionService = new Mock<IMentionService>();
            this._mockDirectMessageService = new Mock<IDirectMessageService>();
            this._mockConversationRepository = new Mock<IConversationRepository>();
            this._mockSearchService = new Mock<ISearchService>();
            this._currentUserId = Guid.NewGuid();

            this._viewModel = new ChatViewModel(
                this._mockMessageService.Object,
                this._mockInteractionService.Object,
                this._mockReadReceiptService.Object,
                this._mockMentionService.Object,
                this._mockDirectMessageService.Object,
                this._mockConversationRepository.Object,
                this._mockSearchService.Object,
                this._currentUserId);
        }

        [Fact]
        public async Task LoadAsync_WithValidConversation_PopulatesMessages()
        {
            var conversationId = Guid.NewGuid();
            var messages = new List<Message>
            {
                new Message { Id = Guid.NewGuid(), Content = "Hello", MessageType = MessageType.Text }
            };

            this._mockMessageService
                .Setup(s => s.GetMessagesAsync(conversationId, this._currentUserId, 0, 100))
                .ReturnsAsync(messages);

            await this._viewModel.LoadAsync(conversationId);

            // Verificăm doar dacă ViewModel-ul și-a populat colecția corect
            Assert.Single(this._viewModel.Messages);
        }

        [Fact]
        public async Task SendCommand_WithEmptyInput_SetsErrorMessage()
        {
            var conversationId = Guid.NewGuid();

            // Simulam încarcarea pentru a seta ConversationId
            this._mockMessageService.Setup(s => s.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Message>());
            await this._viewModel.LoadAsync(conversationId);

            this._viewModel.MessageInput = "    "; // Blank input

            this._viewModel.SendCommand.Execute(null);

            // Un singur Assert - ne asteptam sa seteze o eroare pentru input gol
            Assert.NotNull(this._viewModel.ErrorMessage);
        }

        [Fact]
        public async Task SendCommand_WithValidInput_CallsMessageService()
        {
            var conversationId = Guid.NewGuid();
            var validContent = "Test Message";

            this._mockMessageService.Setup(s => s.GetMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Message>());
            await this._viewModel.LoadAsync(conversationId);

            var createdMessage = new Message { Id = Guid.NewGuid(), Content = validContent, MessageType = MessageType.Text };
            this._mockMessageService
                .Setup(s => s.SendMessageAsync(conversationId, this._currentUserId, validContent, null))
                .ReturnsAsync(createdMessage);

            this._viewModel.MessageInput = validContent;

            this._viewModel.SendCommand.Execute(null);

            // Verificăm dacă serviciul de trimitere a fost apelat corect o singură dată
            this._mockMessageService.Verify(
                s => s.SendMessageAsync(conversationId, this._currentUserId, validContent, null),
                Times.Once);
        }

        [Fact]
        public async Task DeleteMessageCommand_ValidMessage_CallsMessageService()
        {
            var messageId = Guid.NewGuid();

            this._viewModel.DeleteMessageCommand.Execute(messageId);

            // Un singur Assert - serviciul să fie apelat
            this._mockMessageService.Verify(
                s => s.DeleteMessageAsync(messageId, this._currentUserId),
                Times.Once);
        }
    }
}