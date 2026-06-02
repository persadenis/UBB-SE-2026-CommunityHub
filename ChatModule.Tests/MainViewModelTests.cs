/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.Repositories;
using ChatModule.Services;
using ChatModule.src.domain;
using ChatModule.src.domain.Enums;
using ChatModule.src.Interfaces.Repositories;
using ChatModule.src.view_models;
using ChatModule.ViewModels;
using Moq;
using Xunit;

namespace ChatModule.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void Constructor_ShortOverload_CreatesGoToConversationsCommand()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);

            var viewModel = new MainViewModel(
                dependencies.ConversationListService,
                dependencies.FriendRequestService,
                dependencies.BlockService,
                dependencies.ProfileServiceMock.Object,
                dependencies.DirectMessageServiceMock.Object);

            Assert.NotNull(viewModel.GoToConversationsCommand);
        }

        [Fact]
        public void Constructor_NullConversationListService_ThrowsArgumentNullException()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);

            Assert.Throws<ArgumentNullException>(() => new MainViewModel(
                null!,
                dependencies.FriendRequestService,
                dependencies.FriendListService,
                dependencies.BlockService,
                dependencies.ProfileServiceMock.Object,
                dependencies.DirectMessageServiceMock.Object));
        }

        [Fact]
        public void Constructor_NullFriendRequestService_ThrowsArgumentNullException()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);

            Assert.Throws<ArgumentNullException>(() => new MainViewModel(
                dependencies.ConversationListService,
                null!,
                dependencies.FriendListService,
                dependencies.BlockService,
                dependencies.ProfileServiceMock.Object,
                dependencies.DirectMessageServiceMock.Object));
        }

        [Fact]
        public void Constructor_NullBlockService_ThrowsArgumentNullException()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);

            Assert.Throws<ArgumentNullException>(() => new MainViewModel(
                dependencies.ConversationListService,
                dependencies.FriendRequestService,
                dependencies.FriendListService,
                null!,
                dependencies.ProfileServiceMock.Object,
                dependencies.DirectMessageServiceMock.Object));
        }

        [Fact]
        public void Constructor_NullProfileService_ThrowsArgumentNullException()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);

            Assert.Throws<ArgumentNullException>(() => new MainViewModel(
                dependencies.ConversationListService,
                dependencies.FriendRequestService,
                dependencies.FriendListService,
                dependencies.BlockService,
                null!,
                dependencies.DirectMessageServiceMock.Object));
        }

        [Fact]
        public void Constructor_NullDirectMessageService_ThrowsArgumentNullException()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);

            Assert.Throws<ArgumentNullException>(() => new MainViewModel(
                dependencies.ConversationListService,
                dependencies.FriendRequestService,
                dependencies.FriendListService,
                dependencies.BlockService,
                dependencies.ProfileServiceMock.Object,
                null!));
        }

        [Fact]
        public async Task InitialiseAsync_SetsCurrentUserId()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;
            var userId = Guid.NewGuid();

            await viewModel.InitialiseAsync(userId, "user");

            Assert.Equal(userId, viewModel.CurrentUserId);
        }

        [Fact]
        public async Task InitialiseAsync_SetsCurrentUsername()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user-name");

            Assert.Equal("user-name", viewModel.CurrentUsername);
        }

        [Fact]
        public async Task InitialiseAsync_SetsConversationListPage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");

            Assert.IsType<ConversationListViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public async Task GoToConversationsCommand_SetsConversationListPage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToConversationsCommand.ExecuteAsync();

            Assert.IsType<ConversationListViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public async Task GoToFriendsCommand_WithoutFriendListService_SetsFriendRequestsPage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: false);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToFriendsCommand.ExecuteAsync();

            Assert.IsType<FriendRequestsViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public async Task GoToFriendsCommand_WithFriendListService_SetsFriendListPage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToFriendsCommand.ExecuteAsync();

            Assert.IsType<FriendListViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public async Task FriendList_OpenRequestsCommand_SetsFriendRequestsPage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToFriendsCommand.ExecuteAsync();
            var friendListPage = (FriendListViewModel)viewModel.CurrentPage!;
            await friendListPage.OpenRequestsCommand.ExecuteAsync();

            Assert.IsType<FriendRequestsViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public async Task FriendRequests_BackCommand_ReturnsToFriendListPage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToFriendsCommand.ExecuteAsync();
            var friendListPage = (FriendListViewModel)viewModel.CurrentPage!;
            await friendListPage.OpenRequestsCommand.ExecuteAsync();
            var requestsPage = (FriendRequestsViewModel)viewModel.CurrentPage!;
            await requestsPage.BackCommand.ExecuteAsync();

            Assert.IsType<FriendListViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public async Task FriendList_OpenDirectMessageCommand_RaisesNavigateToChatRequested()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;
            var raisedConversationId = Guid.Empty;
            var targetUserId = Guid.NewGuid();

            viewModel.NavigateToChatRequested += conversationId => raisedConversationId = conversationId;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToFriendsCommand.ExecuteAsync();
            var friendListPage = (FriendListViewModel)viewModel.CurrentPage!;
            await friendListPage.OpenDirectMessageCommand.ExecuteAsync(targetUserId);

            Assert.Equal(dependencies.DirectMessageConversationId, raisedConversationId);
        }

        [Fact]
        public async Task FriendList_ViewProfileCommand_SetsProfilePage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToFriendsCommand.ExecuteAsync();
            var friendListPage = (FriendListViewModel)viewModel.CurrentPage!;
            await friendListPage.ViewProfileCommand.ExecuteAsync(Guid.NewGuid());
            await WaitUntilAsync(() => viewModel.CurrentPage is ProfileViewModel);

            Assert.IsType<ProfileViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public async Task GoToProfileCommand_SetsProfilePage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToProfileCommand.ExecuteAsync();

            Assert.IsType<ProfileViewModel>(viewModel.CurrentPage);
        }

        [Fact]
        public async Task LogoutCommand_WithUser_RaisesNavigateToLoginRequested()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;
            var raised = false;

            viewModel.NavigateToLoginRequested += () => raised = true;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.LogoutCommand.ExecuteAsync();

            Assert.True(raised);
        }

        [Fact]
        public async Task LogoutCommand_WithUser_ClearsCurrentUserId()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.LogoutCommand.ExecuteAsync();

            Assert.Equal(Guid.Empty, viewModel.CurrentUserId);
        }

        [Fact]
        public async Task LogoutCommand_WithUser_ClearsCurrentUsername()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.LogoutCommand.ExecuteAsync();

            Assert.Equal(string.Empty, viewModel.CurrentUsername);
        }

        [Fact]
        public async Task LogoutCommand_WithUser_ClearsCurrentPage()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.LogoutCommand.ExecuteAsync();

            Assert.Null(viewModel.CurrentPage);
        }

        [Fact]
        public async Task LogoutCommand_WithUser_CallsUpdateStatusOffline()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.LogoutCommand.ExecuteAsync();

            dependencies.ProfileServiceMock.Verify(
                service => service.UpdateStatusAsync(It.IsAny<Guid>(), UserStatus.Offline),
                Times.Once);

            Assert.True(true);
        }

        [Fact]
        public async Task LogoutCommand_WithoutUser_DoesNotCallUpdateStatus()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.LogoutCommand.ExecuteAsync();

            dependencies.ProfileServiceMock.Verify(
                service => service.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<UserStatus>()),
                Times.Never);

            Assert.True(true);
        }

        [Fact]
        public async Task FriendList_OpenDirectMessageCommand_WithoutMainSubscriber_DoesNotChangePageType()
        {
            var dependencies = this.CreateDependencies(includeFriendListService: true);
            var viewModel = dependencies.ViewModel;

            await viewModel.InitialiseAsync(Guid.NewGuid(), "user");
            await viewModel.GoToFriendsCommand.ExecuteAsync();
            var friendListPage = (FriendListViewModel)viewModel.CurrentPage!;

            await friendListPage.OpenDirectMessageCommand.ExecuteAsync(Guid.NewGuid());

            Assert.IsType<FriendListViewModel>(viewModel.CurrentPage);
        }

        private static async Task WaitUntilAsync(Func<bool> condition)
        {
            var timeout = TimeSpan.FromSeconds(1);
            var stopwatch = Stopwatch.StartNew();

            while (!condition() && stopwatch.Elapsed < timeout)
            {
                await Task.Delay(10);
            }
        }

        private MainViewModelDependencies CreateDependencies(bool includeFriendListService)
        {
            var friendRepositoryMock = new Mock<IFriendRepository>();
            friendRepositoryMock
                .Setup(repository => repository.GetFriendshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Friend?)null);

            var userRepositoryMock = new Mock<IUserRepository>();

            var profileServiceMock = new Mock<IProfileService>();
            profileServiceMock
                .Setup(service => service.GetProfileAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid userId) => new User
                {
                    Id = userId,
                    Username = "User",
                    Status = UserStatus.Online
                });
            profileServiceMock
                .Setup(service => service.GetMutualFriendsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new List<User>());
            profileServiceMock
                .Setup(service => service.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<UserStatus>()))
                .Returns(Task.CompletedTask);

            var directMessageConversationId = Guid.NewGuid();
            var directMessageServiceMock = new Mock<IDirectMessageService>();
            directMessageServiceMock
                .Setup(service => service.GetOrCreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Conversation
                {
                    Id = directMessageConversationId
                });

            var conversationListService = new ConversationListService(
                null!,
                null!,
                null!,
                userRepositoryMock.Object);

            var friendRequestService = new FriendRequestService(
                friendRepositoryMock.Object,
                userRepositoryMock.Object,
                null!,
                null!);

            FriendListService? friendListService = includeFriendListService
                ? new FriendListService(friendRepositoryMock.Object, userRepositoryMock.Object)
                : null;

            var blockService = new BlockService(friendRepositoryMock.Object, userRepositoryMock.Object);

            var viewModel = new MainViewModel(
                conversationListService,
                friendRequestService,
                friendListService,
                blockService,
                profileServiceMock.Object,
                directMessageServiceMock.Object);

            return new MainViewModelDependencies
            {
                ViewModel = viewModel,
                ConversationListService = conversationListService,
                FriendRequestService = friendRequestService,
                FriendListService = friendListService,
                BlockService = blockService,
                ProfileServiceMock = profileServiceMock,
                DirectMessageServiceMock = directMessageServiceMock,
                DirectMessageConversationId = directMessageConversationId
            };
        }

        private sealed class MainViewModelDependencies
        {
            public required MainViewModel ViewModel { get; init; }
            public required ConversationListService ConversationListService { get; init; }
            public required FriendRequestService FriendRequestService { get; init; }
            public FriendListService? FriendListService { get; init; }
            public required BlockService BlockService { get; init; }
            public required Mock<IProfileService> ProfileServiceMock { get; init; }
            public required Mock<IDirectMessageService> DirectMessageServiceMock { get; init; }
            public required Guid DirectMessageConversationId { get; init; }
        }
    }
}
*/