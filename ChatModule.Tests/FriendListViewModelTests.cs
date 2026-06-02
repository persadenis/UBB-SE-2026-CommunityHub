using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.src.domain;
using ChatModule.src.view_models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ChatModule.Tests
{
    public class FriendListViewModelTests
    {
        private readonly Mock<IFriendListService> mockFriendListService;
        private readonly Mock<IFriendRequestService> mockFriendRequestService;
        private readonly Mock<IDirectMessageService> mockDirectMessageService;
        private readonly FriendListViewModel viewModel;
        private readonly Guid currentUserId;

        public FriendListViewModelTests()
        {
            mockFriendListService = new Mock<IFriendListService>();
            mockFriendRequestService = new Mock<IFriendRequestService>();
            mockDirectMessageService = new Mock<IDirectMessageService>();
            currentUserId = Guid.NewGuid();

            viewModel = new FriendListViewModel(
                mockFriendListService.Object,
                mockFriendRequestService.Object,
                mockDirectMessageService.Object,
                currentUserId);
        }

        // Constructor

        [Fact]
        public void Constructor_NullFriendListService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FriendListViewModel(null!, mockFriendRequestService.Object, currentUserId));
        }

        [Fact]
        public void Constructor_NullFriendRequestService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FriendListViewModel(mockFriendListService.Object, null!, currentUserId));
        }

        [Fact]
        public void Constructor_NullDirectMessageService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FriendListViewModel(mockFriendListService.Object, mockFriendRequestService.Object, null!, currentUserId));
        }

        [Fact]
        public void SetDirectMessageService_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => viewModel.SetDirectMessageService(null!));
        }

        [Fact]
        public void SetDirectMessageService_ValidService_AssignsSuccessfully()
        {
            var exception = Record.Exception(() => viewModel.SetDirectMessageService(mockDirectMessageService.Object));
            Assert.Null(exception);
        }

        // LoadCommand

        [Fact]
        public async Task LoadCommand_Success_PopulatesFriendsList()
        {
            var mockFriends = new List<User> { new User { Id = Guid.NewGuid(), Username = "Friend1", Status = UserStatus.Online } };
            mockFriendListService.Setup(s => s.GetFriendsAsync(currentUserId)).ReturnsAsync(mockFriends);

            await viewModel.LoadCommand.ExecuteAsync(null);

            Assert.Single(viewModel.Friends);
        }

        [Fact]
        public async Task LoadCommand_Success_SetsHasFriendsTrue()
        {
            var mockFriends = new List<User> { new User { Id = Guid.NewGuid(), Username = "Friend1", Status = UserStatus.Online } };
            mockFriendListService.Setup(s => s.GetFriendsAsync(currentUserId)).ReturnsAsync(mockFriends);

            await viewModel.LoadCommand.ExecuteAsync(null);

            Assert.True(viewModel.HasFriends);
        }

        [Fact]
        public async Task LoadCommand_EmptyList_LeavesFriendsListEmpty()
        {
            mockFriendListService.Setup(s => s.GetFriendsAsync(currentUserId)).ReturnsAsync(new List<User>());

            await viewModel.LoadCommand.ExecuteAsync(null);

            Assert.Empty(viewModel.Friends);
        }

        [Fact]
        public async Task LoadCommand_EmptyList_SetsShowEmptyStateTrue()
        {
            mockFriendListService.Setup(s => s.GetFriendsAsync(currentUserId)).ReturnsAsync(new List<User>());

            await viewModel.LoadCommand.ExecuteAsync(null);

            Assert.True(viewModel.ShowEmptyState);
        }

        [Fact]
        public async Task LoadCommand_ServiceThrowsException_IsLoadingIsFalse()
        {
            mockFriendListService.Setup(s => s.GetFriendsAsync(currentUserId)).ThrowsAsync(new Exception("Error"));

            await Record.ExceptionAsync(() => viewModel.LoadCommand.ExecuteAsync(null));

            Assert.False(viewModel.IsLoading);
        }

        // OpenDirectMessageCommand

        [Fact]
        public async Task OpenDirectMessageCommand_EmptyId_PreventsNavigation()
        {
            bool navigated = false;
            viewModel.NavigateToChatRequested += (id) => navigated = true;

            await viewModel.OpenDirectMessageCommand.ExecuteAsync(Guid.Empty);

            Assert.False(navigated);
        }

        [Fact]
        public async Task OpenDirectMessageCommand_NullDmService_PreventsNavigation()
        {
            var noDmViewModel = new FriendListViewModel(mockFriendListService.Object, mockFriendRequestService.Object, currentUserId);
            bool navigated = false;
            noDmViewModel.NavigateToChatRequested += (id) => navigated = true;

            await noDmViewModel.OpenDirectMessageCommand.ExecuteAsync(Guid.NewGuid());

            Assert.False(navigated);
        }

        [Fact]
        public async Task OpenDirectMessageCommand_ValidTargetNoSubscribers_DoesNotThrow()
        {
            var targetId = Guid.NewGuid();
            mockDirectMessageService.Setup(s => s.GetOrCreateAsync(currentUserId, targetId)).ReturnsAsync(new Conversation());

            var exception = await Record.ExceptionAsync(() => viewModel.OpenDirectMessageCommand.ExecuteAsync(targetId));

            Assert.Null(exception);
        }

        [Fact]
        public async Task OpenDirectMessageCommand_ValidTarget_NavigatesToConversation()
        {
            var targetId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            mockDirectMessageService.Setup(s => s.GetOrCreateAsync(currentUserId, targetId)).ReturnsAsync(new Conversation { Id = conversationId });
            Guid? navigatedToId = null;
            viewModel.NavigateToChatRequested += (id) => navigatedToId = id;

            await viewModel.OpenDirectMessageCommand.ExecuteAsync(targetId);

            Assert.Equal(conversationId, navigatedToId);
        }

        // ViewProfileCommand

        [Fact]
        public async Task ViewProfileCommand_EmptyId_DoesNothing()
        {
            bool navigated = false;
            viewModel.NavigateToProfileRequested += (id) => navigated = true;

            await viewModel.ViewProfileCommand.ExecuteAsync(Guid.Empty);

            Assert.False(navigated);
        }

        [Fact]
        public async Task ViewProfileCommand_NoSubscribers_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => viewModel.ViewProfileCommand.ExecuteAsync(Guid.NewGuid()));
            Assert.Null(exception);
        }

        [Fact]
        public async Task ViewProfileCommand_ValidTarget_NavigatesToProfile()
        {
            var targetId = Guid.NewGuid();
            Guid? navigatedToId = null;
            viewModel.NavigateToProfileRequested += (id) => navigatedToId = id;

            await viewModel.ViewProfileCommand.ExecuteAsync(targetId);

            Assert.Equal(targetId, navigatedToId);
        }

        // RemoveFriendCommand

        [Fact]
        public async Task RemoveFriendCommand_EmptyId_NeverCallsService()
        {
            await viewModel.RemoveFriendCommand.ExecuteAsync(Guid.Empty);

            mockFriendListService.Verify(s => s.RemoveFriendAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RemoveFriendCommand_ValidTarget_CallsService()
        {
            var targetId = Guid.NewGuid();
            mockFriendListService.Setup(s => s.GetFriendsAsync(currentUserId)).ReturnsAsync(new List<User>());

            await viewModel.RemoveFriendCommand.ExecuteAsync(targetId);

            mockFriendListService.Verify(s => s.RemoveFriendAsync(currentUserId, targetId), Times.Once);
        }

        // SendFriendRequestCommand

        [Fact]
        public async Task SendFriendRequestCommand_EmptyInput_SetsErrorMessage()
        {
            viewModel.FriendUsernameInput = "   ";

            await viewModel.SendFriendRequestCommand.ExecuteAsync(null);

            Assert.Equal("Enter a username first.", viewModel.FriendActionMessage);
        }

        [Fact]
        public async Task SendFriendRequestCommand_UserNotFound_SetsErrorMessage()
        {
            viewModel.FriendUsernameInput = "GhostUser";
            mockFriendRequestService.Setup(s => s.SendFriendRequestByUsernameAsync(currentUserId, "GhostUser")).ReturnsAsync(false);

            await viewModel.SendFriendRequestCommand.ExecuteAsync(null);

            Assert.Equal("User not found.", viewModel.FriendActionMessage);
        }

        [Fact]
        public async Task SendFriendRequestCommand_Success_SetsSuccessMessage()
        {
            viewModel.FriendUsernameInput = "ValidUser";
            mockFriendRequestService.Setup(s => s.SendFriendRequestByUsernameAsync(currentUserId, "ValidUser")).ReturnsAsync(true);

            await viewModel.SendFriendRequestCommand.ExecuteAsync(null);

            Assert.Equal("Friend request sent.", viewModel.FriendActionMessage);
        }

        [Fact]
        public async Task SendFriendRequestCommand_Success_ClearsInput()
        {
            viewModel.FriendUsernameInput = "ValidUser";
            mockFriendRequestService.Setup(s => s.SendFriendRequestByUsernameAsync(currentUserId, "ValidUser")).ReturnsAsync(true);

            await viewModel.SendFriendRequestCommand.ExecuteAsync(null);

            Assert.Equal(string.Empty, viewModel.FriendUsernameInput);
        }

        [Fact]
        public async Task SendFriendRequestCommand_ExceptionThrown_SetsErrorMessage()
        {
            viewModel.FriendUsernameInput = "ErrorUser";
            mockFriendRequestService.Setup(s => s.SendFriendRequestByUsernameAsync(currentUserId, "ErrorUser")).ThrowsAsync(new Exception("Database Error"));

            await viewModel.SendFriendRequestCommand.ExecuteAsync(null);

            Assert.Equal("Database Error", viewModel.FriendActionMessage);
        }

        // OpenRequestsCommand

        [Fact]
        public async Task OpenRequestsCommand_NoSubscribers_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() => viewModel.OpenRequestsCommand.ExecuteAsync(null));

            Assert.Null(exception);
        }

        [Fact]
        public async Task OpenRequestsCommand_WithSubscribers_RaisesEvent()
        {
            bool navigated = false;
            viewModel.OpenRequestsRequested += () => navigated = true;

            await viewModel.OpenRequestsCommand.ExecuteAsync(null);

            Assert.True(navigated);
        }
        // UI
        [Fact]
        public void Properties_SetToSameValue_DoesNotTriggerChange()
        {
            viewModel.IsLoading = true;
            bool propertyChanged = false;
            viewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            viewModel.IsLoading = true;

            Assert.False(propertyChanged);
        }

        [Fact]
        public void DisplayProperties_WhenLoading_ShowFriendListIsFalse()
        {
            viewModel.IsLoading = true;
            Assert.False(viewModel.ShowFriendList);
        }

        [Fact]
        public void DisplayProperties_WhenLoading_ShowEmptyStateIsFalse()
        {
            viewModel.IsLoading = true;
            Assert.False(viewModel.ShowEmptyState);
        }

        [Fact]
        public void DisplayProperties_NotLoadingAndEmpty_ShowFriendListIsFalse()
        {
            viewModel.IsLoading = false;
            Assert.False(viewModel.ShowFriendList);
        }

        [Fact]
        public void DisplayProperties_NotLoadingAndEmpty_ShowEmptyStateIsTrue()
        {
            viewModel.IsLoading = false;
            Assert.True(viewModel.ShowEmptyState);
        }

        [Fact]
        public void DisplayProperties_NotLoadingWithFriends_ShowFriendListIsTrue()
        {
            var dummyUser = new User { Id = Guid.NewGuid(), Username = "Test" };
            viewModel.FriendItemViewModels.Add(new FriendListItemViewModel(dummyUser));

            Assert.True(viewModel.ShowFriendList);
        }

        [Fact]
        public void DisplayProperties_NotLoadingWithFriends_ShowEmptyStateIsFalse()
        {
            var dummyUser = new User { Id = Guid.NewGuid(), Username = "Test" };
            viewModel.FriendItemViewModels.Add(new FriendListItemViewModel(dummyUser));

            Assert.False(viewModel.ShowEmptyState);
        }
    }
}