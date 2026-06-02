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
    public class MemberPanelViewModelTests
    {
        private readonly Mock<IMemberPanelService> _memberPanelServiceMock = new();
        private readonly Mock<IModerationService> _moderationServiceMock = new();
        private readonly Guid _currentUserId = Guid.NewGuid();

        private MemberPanelViewModel CreateViewModel()
            => new(_memberPanelServiceMock.Object, _moderationServiceMock.Object, _currentUserId);

        [Fact]
        public void Constructor_NullMemberPanelService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberPanelViewModel(null!, _moderationServiceMock.Object, _currentUserId));
        }

        [Fact]
        public void Constructor_NullModerationService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberPanelViewModel(_memberPanelServiceMock.Object, null!, _currentUserId));
        }

        [Fact]
        public async Task LoadAsync_PopulatesMembersBannedMembersAndAdminFlag()
        {
            var conversationId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var bannedId = Guid.NewGuid();
            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { UserId = _currentUserId, Role = ParticipantRole.Admin },
                    new() { UserId = memberId, Role = ParticipantRole.Member },
                    new() { UserId = bannedId, Role = ParticipantRole.Banned },
                });
            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(_currentUserId))
                .ReturnsAsync(new User { Id = _currentUserId, Username = "me" });
            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(memberId))
                .ReturnsAsync(new User { Id = memberId, Username = "member" });
            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(bannedId))
                .ReturnsAsync(new User { Id = bannedId, Username = "banned" });

            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);

            Assert.True(viewModel.ConversationId == conversationId && viewModel.Members.Count == 2 && viewModel.BannedMembers.Count == 1 && viewModel.IsAdmin);
        }

        [Fact]
        public async Task LoadAsync_SkipsParticipantsWithoutUser()
        {
            var conversationId = Guid.NewGuid();
            var missingUserId = Guid.NewGuid();
            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant> { new() { UserId = missingUserId, Role = ParticipantRole.Member } });
            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(missingUserId))
                .ReturnsAsync((User?)null);

            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);

            Assert.Empty(viewModel.Members);
        }

        [Fact]
        public async Task LoadCommand_ServiceThrows_SetsErrorMessage()
        {
            var viewModel = CreateViewModel();
            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(Guid.Empty))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.LoadCommand.ExecuteAsync(null);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadCommand_AfterFailure_StopsLoading()
        {
            var viewModel = CreateViewModel();
            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(Guid.Empty))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.LoadCommand.ExecuteAsync(null);

            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public async Task TogglePanelCommand_TogglesPanelVisibility()
        {
            var viewModel = CreateViewModel();

            await viewModel.TogglePanelCommand.ExecuteAsync(null);

            Assert.False(viewModel.IsPanelVisible);
        }

        [Fact]
        public void ShowHeader_False_MakesMemberContentVisible()
        {
            var viewModel = CreateViewModel();
            viewModel.IsPanelVisible = false;
            viewModel.ShowHeader = false;

            Assert.True(viewModel.ShowMemberContent);
        }

        [Fact]
        public void TogglePanelIcon_WhenPanelVisible_ReturnsLeftArrow()
        {
            var viewModel = CreateViewModel();

            Assert.Equal("◀", viewModel.TogglePanelIcon);
        }

        [Fact]
        public async Task AddMemberQuery_Whitespace_ClearsResultsAndSelection()
        {
            var viewModel = CreateViewModel();
            viewModel.AddMemberResults.Add(new User());
            viewModel.SelectedAddMember = new User();

            viewModel.AddMemberQuery = "   ";
            await Task.Yield();

            Assert.True(viewModel.AddMemberResults.Count == 0 && viewModel.SelectedAddMember == null);
        }

        [Fact]
        public async Task AddMemberQuery_WithText_PopulatesSearchResults()
        {
            var conversationId = Guid.NewGuid();
            var foundUser = new User { Id = Guid.NewGuid(), Username = "anna" };
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);

            _memberPanelServiceMock
                .Setup(service => service.SearchUsersToAddAsync(conversationId, "ann"))
                .ReturnsAsync(new List<User> { foundUser });

            viewModel.AddMemberQuery = "ann";
            await Task.Delay(20);

            Assert.Equal(foundUser.Id, viewModel.AddMemberResults[0].Id);
        }

        [Fact]
        public async Task AddMemberCommand_NoSelection_DoesNothing()
        {
            var viewModel = CreateViewModel();
            var wasCalled = false;
            _moderationServiceMock
                .Setup(service => service.AddMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => wasCalled = true)
                .Returns(Task.CompletedTask);

            await viewModel.AddMemberCommand.ExecuteAsync(null);

            Assert.False(wasCalled);
        }

        

        [Fact]
        public async Task AddMemberCommand_ServiceThrows_SetsErrorMessage()
        {
            var conversationId = Guid.NewGuid();
            var selectedUser = new User { Id = Guid.NewGuid(), Username = "anna" };
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            viewModel.SelectedAddMember = selectedUser;
            _moderationServiceMock
                .Setup(service => service.AddMemberAsync(conversationId, _currentUserId, selectedUser.Id))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.AddMemberCommand.ExecuteAsync(null);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task BanMemberCommand_EmptyUserId_DoesNothing()
        {
            var viewModel = CreateViewModel();
            var wasCalled = false;
            _moderationServiceMock
                .Setup(service => service.BanMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => wasCalled = true)
                .Returns(Task.CompletedTask);

            await viewModel.BanMemberCommand.ExecuteAsync(Guid.Empty);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task BanMemberCommand_ServiceThrows_SetsErrorMessage()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            _moderationServiceMock
                .Setup(service => service.BanMemberAsync(conversationId, _currentUserId, targetId))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.BanMemberCommand.ExecuteAsync(targetId);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task UnbanMemberCommand_EmptyUserId_DoesNothing()
        {
            var viewModel = CreateViewModel();
            var wasCalled = false;
            _moderationServiceMock
                .Setup(service => service.UnbanMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => wasCalled = true)
                .Returns(Task.CompletedTask);

            await viewModel.UnbanMemberCommand.ExecuteAsync(Guid.Empty);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task UnbanMemberCommand_ServiceThrows_SetsErrorMessage()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            _moderationServiceMock
                .Setup(service => service.UnbanMemberAsync(conversationId, _currentUserId, targetId))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.UnbanMemberCommand.ExecuteAsync(targetId);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task TimeoutMemberCommand_EmptyUserId_DoesNothing()
        {
            var viewModel = CreateViewModel();
            var wasCalled = false;
            _moderationServiceMock
                .Setup(service => service.TimeoutMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
                .Callback(() => wasCalled = true)
                .Returns(Task.CompletedTask);

            await viewModel.TimeoutMemberCommand.ExecuteAsync(Guid.Empty);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task TimeoutMemberCommand_NullRequestedDuration_DoesNothing()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var wasCalled = false;
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            viewModel.RequestTimeoutDurationAsync = () => Task.FromResult<TimeSpan?>(null);
            _moderationServiceMock
                .Setup(service => service.TimeoutMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
                .Callback(() => wasCalled = true)
                .Returns(Task.CompletedTask);

            await viewModel.TimeoutMemberCommand.ExecuteAsync(targetId);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task TimeoutMemberCommand_WithoutCallback_UsesDefaultDuration()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var usedDuration = TimeSpan.Zero;
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            _moderationServiceMock
                .Setup(service => service.TimeoutMemberAsync(conversationId, _currentUserId, targetId, It.IsAny<TimeSpan>()))
                .Callback<Guid, Guid, Guid, TimeSpan>((_, _, _, duration) => usedDuration = duration)
                .Returns(Task.CompletedTask);
            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>());

            await viewModel.TimeoutMemberCommand.ExecuteAsync(targetId);

            Assert.Equal(TimeSpan.FromMinutes(10), usedDuration);
        }

        [Fact]
        public async Task TimeoutMemberCommand_WithCallback_UsesCustomDuration()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var usedDuration = TimeSpan.Zero;
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            viewModel.RequestTimeoutDurationAsync = () => Task.FromResult<TimeSpan?>(TimeSpan.FromHours(2));
            _moderationServiceMock
                .Setup(service => service.TimeoutMemberAsync(conversationId, _currentUserId, targetId, It.IsAny<TimeSpan>()))
                .Callback<Guid, Guid, Guid, TimeSpan>((_, _, _, duration) => usedDuration = duration)
                .Returns(Task.CompletedTask);
            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>());

            await viewModel.TimeoutMemberCommand.ExecuteAsync(targetId);

            Assert.Equal(TimeSpan.FromHours(2), usedDuration);
        }

        [Fact]
        public async Task TimeoutMemberCommand_ServiceThrows_SetsErrorMessage()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            _moderationServiceMock
                .Setup(service => service.TimeoutMemberAsync(conversationId, _currentUserId, targetId, It.IsAny<TimeSpan>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.TimeoutMemberCommand.ExecuteAsync(targetId);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task RemoveTimeoutCommand_EmptyUserId_DoesNothing()
        {
            var viewModel = CreateViewModel();
            var wasCalled = false;
            _moderationServiceMock
                .Setup(service => service.RemoveTimeoutAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => wasCalled = true)
                .Returns(Task.CompletedTask);

            await viewModel.RemoveTimeoutCommand.ExecuteAsync(Guid.Empty);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task RemoveTimeoutCommand_ServiceThrows_SetsErrorMessage()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            _moderationServiceMock
                .Setup(service => service.RemoveTimeoutAsync(conversationId, _currentUserId, targetId))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.RemoveTimeoutCommand.ExecuteAsync(targetId);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task PromoteCommand_EmptyUserId_DoesNothing()
        {
            var viewModel = CreateViewModel();
            var wasCalled = false;
            _moderationServiceMock
                .Setup(service => service.PromoteMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => wasCalled = true)
                .Returns(Task.CompletedTask);

            await viewModel.PromoteCommand.ExecuteAsync(Guid.Empty);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task PromoteCommand_ServiceThrows_SetsErrorMessage()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            _moderationServiceMock
                .Setup(service => service.PromoteMemberAsync(conversationId, _currentUserId, targetId))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.PromoteCommand.ExecuteAsync(targetId);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task DemoteCommand_EmptyUserId_DoesNothing()
        {
            var viewModel = CreateViewModel();
            var wasCalled = false;
            _moderationServiceMock
                .Setup(service => service.DemoteMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Callback(() => wasCalled = true)
                .Returns(Task.CompletedTask);

            await viewModel.DemoteCommand.ExecuteAsync(Guid.Empty);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task DemoteCommand_ServiceThrows_SetsErrorMessage()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();
            await viewModel.LoadAsync(conversationId);
            _moderationServiceMock
                .Setup(service => service.DemoteMemberAsync(conversationId, _currentUserId, targetId))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.DemoteCommand.ExecuteAsync(targetId);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task ViewProfileCommand_EmptyUserId_DoesNotRaiseEvent()
        {
            var viewModel = CreateViewModel();
            var raised = false;
            viewModel.NavigateToProfileRequested += _ => raised = true;

            await viewModel.ViewProfileCommand.ExecuteAsync(Guid.Empty);

            Assert.False(raised);
        }

        [Fact]
        public async Task ViewProfileCommand_ValidUserId_RaisesEvent()
        {
            var userId = Guid.NewGuid();
            var viewModel = CreateViewModel();
            Guid? navigatedId = null;
            viewModel.NavigateToProfileRequested += id => navigatedId = id;

            await viewModel.ViewProfileCommand.ExecuteAsync(userId);

            Assert.Equal(userId, navigatedId);
        }
        
        [Fact]
        public void TogglePanelIcon_WhenPanelHidden_ReturnsRightArrow()
        {
            var viewModel = CreateViewModel();
            viewModel.IsPanelVisible = false;

            Assert.Equal("▶", viewModel.TogglePanelIcon);
        }

        [Fact]
        public async Task AddMemberCommand_Success_ClearsSelectionAndQuery()
        {
            var conversationId = Guid.NewGuid();
            var selectedUser = new User { Id = Guid.NewGuid(), Username = "anna" };
            var viewModel = CreateViewModel();

            _memberPanelServiceMock
                .SetupSequence(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>())
                .ReturnsAsync(new List<Participant>());

            _memberPanelServiceMock
                .Setup(service => service.SearchUsersToAddAsync(conversationId, "ann"))
                .ReturnsAsync(new List<User> { selectedUser });

            await viewModel.LoadAsync(conversationId);

            viewModel.AddMemberQuery = "ann";
            await Task.Delay(20);

            viewModel.SelectedAddMember = selectedUser;

            _moderationServiceMock
                .Setup(service => service.AddMemberAsync(conversationId, _currentUserId, selectedUser.Id))
                .Returns(Task.CompletedTask);

            await viewModel.AddMemberCommand.ExecuteAsync(null);

            Assert.Null(viewModel.SelectedAddMember);
            Assert.Equal(string.Empty, viewModel.AddMemberQuery);
            Assert.Empty(viewModel.AddMemberResults);
        }

        [Fact]
        public async Task BanMemberCommand_Success_ReloadsMembers()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();

            await viewModel.LoadAsync(conversationId);

            _moderationServiceMock
                .Setup(service => service.BanMemberAsync(conversationId, _currentUserId, targetId))
                .Returns(Task.CompletedTask);

            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { UserId = targetId, Role = ParticipantRole.Banned },
                });

            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(targetId))
                .ReturnsAsync(new User { Id = targetId, Username = "banned-user" });

            await viewModel.BanMemberCommand.ExecuteAsync(targetId);

            Assert.Single(viewModel.BannedMembers);
            Assert.Equal(targetId, viewModel.BannedMembers[0].UserId);
        }

        [Fact]
        public async Task UnbanMemberCommand_Success_ReloadsMembers()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();

            await viewModel.LoadAsync(conversationId);

            _moderationServiceMock
                .Setup(service => service.UnbanMemberAsync(conversationId, _currentUserId, targetId))
                .Returns(Task.CompletedTask);

            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { UserId = targetId, Role = ParticipantRole.Member },
                });

            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(targetId))
                .ReturnsAsync(new User { Id = targetId, Username = "member-user" });

            await viewModel.UnbanMemberCommand.ExecuteAsync(targetId);

            Assert.Single(viewModel.Members);
            Assert.Equal(targetId, viewModel.Members[0].UserId);
        }

        [Fact]
        public async Task RemoveTimeoutCommand_Success_ReloadsMembers()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();

            await viewModel.LoadAsync(conversationId);

            _moderationServiceMock
                .Setup(service => service.RemoveTimeoutAsync(conversationId, _currentUserId, targetId))
                .Returns(Task.CompletedTask);

            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { UserId = targetId, Role = ParticipantRole.Member, TimeoutUntil = null },
                });

            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(targetId))
                .ReturnsAsync(new User { Id = targetId, Username = "member-user" });

            await viewModel.RemoveTimeoutCommand.ExecuteAsync(targetId);

            Assert.Single(viewModel.Members);
            Assert.False(viewModel.Members[0].HasTimeout);
        }

        [Fact]
        public async Task PromoteCommand_Success_ReloadsMembers()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();

            await viewModel.LoadAsync(conversationId);

            _moderationServiceMock
                .Setup(service => service.PromoteMemberAsync(conversationId, _currentUserId, targetId))
                .Returns(Task.CompletedTask);

            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { UserId = targetId, Role = ParticipantRole.Admin },
                });

            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(targetId))
                .ReturnsAsync(new User { Id = targetId, Username = "admin-user" });

            await viewModel.PromoteCommand.ExecuteAsync(targetId);

            Assert.Single(viewModel.Members);
            Assert.Equal(ParticipantRole.Admin, viewModel.Members[0].Role);
        }

        [Fact]
        public async Task DemoteCommand_Success_ReloadsMembers()
        {
            var conversationId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var viewModel = CreateViewModel();

            await viewModel.LoadAsync(conversationId);

            _moderationServiceMock
                .Setup(service => service.DemoteMemberAsync(conversationId, _currentUserId, targetId))
                .Returns(Task.CompletedTask);

            _memberPanelServiceMock
                .Setup(service => service.GetMembersAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { UserId = targetId, Role = ParticipantRole.Member },
                });

            _memberPanelServiceMock
                .Setup(service => service.GetUserAsync(targetId))
                .ReturnsAsync(new User { Id = targetId, Username = "member-user" });

            await viewModel.DemoteCommand.ExecuteAsync(targetId);

            Assert.Single(viewModel.Members);
            Assert.Equal(ParticipantRole.Member, viewModel.Members[0].Role);
        }
    }
}
