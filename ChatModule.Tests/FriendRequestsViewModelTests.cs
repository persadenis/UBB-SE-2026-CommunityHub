using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatModule.src.view_models;
using Moq;
using Xunit;

namespace ChatModule.Tests
{
    public class FriendRequestsViewModelTests
    {
        private readonly Mock<IFriendRequestService> mockFriendRequestService;
        private readonly FriendRequestsViewModel viewModel;
        private readonly Guid currentUserId;

        public FriendRequestsViewModelTests()
        {
            mockFriendRequestService = new Mock<IFriendRequestService>();
            currentUserId = Guid.NewGuid();

            mockFriendRequestService
                .Setup(s => s.GetIncomingRequestsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<User>());

            viewModel = new FriendRequestsViewModel(
                mockFriendRequestService.Object,
                currentUserId);
        }

        [Fact]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FriendRequestsViewModel(null!, currentUserId));
        }

        [Fact]
        public async Task LoadCommand_Executes_PopulatesIncomingRequests()
        {
            var mockUsers = new List<User> { new User { Id = Guid.NewGuid(), Username = "Sender" } };
            mockFriendRequestService.Setup(s => s.GetIncomingRequestsAsync(currentUserId))
                .ReturnsAsync(mockUsers);

            await viewModel.LoadCommand.ExecuteAsync(null);

            Assert.Single(viewModel.IncomingRequests);
        }

        [Fact]
        public async Task AcceptCommand_Executes_CallsService()
        {
            var requesterId = Guid.NewGuid();

            await viewModel.AcceptCommand.ExecuteAsync(requesterId);

            mockFriendRequestService.Verify(s => s.AcceptFriendRequestAsync(currentUserId, requesterId), Times.Once);
        }

        [Fact]
        public async Task DeclineCommand_Executes_CallsService()
        {
            var requesterId = Guid.NewGuid();

            await viewModel.DeclineCommand.ExecuteAsync(requesterId);

            mockFriendRequestService.Verify(s => s.DeclineFriendRequestAsync(currentUserId, requesterId), Times.Once);
        }

        [Fact]
        public void BackCommand_Executes_RaisesNavigateBackRequested()
        {
            bool eventRaised = false;
            viewModel.NavigateBackRequested += () => eventRaised = true;

            viewModel.BackCommand.Execute(null);

            Assert.True(eventRaised);
        }

        [Fact]
        public void IsLoading_InitialState_IsFalse()
        {
            Assert.False(viewModel.IsLoading);
        }
        [Fact]
        public void BackCommand_WhenNoSubscribers_DoesNotThrow()
        {
            var exception = Record.Exception(() => viewModel.BackCommand.Execute(null));
            Assert.Null(exception);
        }
    }
}