using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.src.view_models;
using Moq;
using Xunit;

namespace ChatModule.Tests
{
    public class CreateGroupViewModelTests
    {
        private readonly Mock<IGroupService> _groupServiceMock = new();
        private readonly Mock<ISearchService> _searchServiceMock = new();
        private readonly Guid _currentUserId = Guid.NewGuid();

        private CreateGroupViewModel CreateViewModel()
            => new(_groupServiceMock.Object, _searchServiceMock.Object, _currentUserId);

        [Fact]
        public void Constructor_NullGroupService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CreateGroupViewModel(null!, _searchServiceMock.Object, _currentUserId));
        }

        [Fact]
        public void Constructor_NullSearchService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CreateGroupViewModel(_groupServiceMock.Object, null!, _currentUserId));
        }

        [Fact]
        public async Task MemberSearchQuery_Whitespace_LeavesResultsEmpty()
        {
            var viewModel = CreateViewModel();

            viewModel.MemberSearchQuery = "   ";
            await Task.Yield();

            Assert.Empty(viewModel.MemberSearchResults);
        }

        [Fact]
        public async Task MemberSearchQuery_ExcludesCurrentUserAndSelectedMembers()
        {
            var selectedUser = new User { Id = Guid.NewGuid(), Username = "selected" };
            var otherUser = new User { Id = Guid.NewGuid(), Username = "other" };
            var viewModel = CreateViewModel();
            viewModel.SelectedMembers.Add(selectedUser);

            _searchServiceMock
                .Setup(service => service.SearchUsersAsync("john"))
                .ReturnsAsync(new List<User>
                {
                    new() { Id = _currentUserId, Username = "me" },
                    selectedUser,
                    otherUser,
                });

            viewModel.MemberSearchQuery = "john";
            await Task.Delay(20);

            Assert.Single(viewModel.MemberSearchResults);
        }

        [Fact]
        public async Task AddMemberCommand_NewUser_AddsSelectionAndRemovesSearchResult()
        {
            var viewModel = CreateViewModel();
            var user = new User { Id = Guid.NewGuid(), Username = "anna" };
            viewModel.MemberSearchResults.Add(user);

            await viewModel.AddMemberCommand.ExecuteAsync(user);

            Assert.True(viewModel.SelectedMembers.Count == 1 && viewModel.MemberSearchResults.Count == 0);
        }

        [Fact]
        public async Task AddMemberCommand_DuplicateUser_DoesNotDuplicateSelection()
        {
            var viewModel = CreateViewModel();
            var user = new User { Id = Guid.NewGuid(), Username = "anna" };
            viewModel.SelectedMembers.Add(user);
            viewModel.MemberSearchResults.Add(user);

            await viewModel.AddMemberCommand.ExecuteAsync(user);

            Assert.Single(viewModel.SelectedMembers);
        }

        [Fact]
        public async Task RemoveMemberCommand_RemovesUserFromSelectedMembers()
        {
            var viewModel = CreateViewModel();
            var user = new User { Id = Guid.NewGuid(), Username = "anna" };
            viewModel.SelectedMembers.Add(user);

            await viewModel.RemoveMemberCommand.ExecuteAsync(user);

            Assert.Empty(viewModel.SelectedMembers);
        }

        [Fact]
        public async Task CreateCommand_Success_RaisesGroupCreated()
        {
            var createdConversation = new Conversation { Id = Guid.NewGuid(), Title = "Team" };
            var member = new User { Id = Guid.NewGuid(), Username = "anna" };
            var viewModel = CreateViewModel();
            viewModel.GroupName = "Team";
            viewModel.IconUrl = "icon.png";
            viewModel.SelectedMembers.Add(member);

            Conversation? raisedConversation = null;
            viewModel.GroupCreated += conversation => raisedConversation = conversation;

            _groupServiceMock
                .Setup(service => service.CreateGroupAsync(_currentUserId, "Team", "icon.png", It.Is<List<Guid>>(ids => ids.SequenceEqual(new[] { member.Id }))))
                .ReturnsAsync(createdConversation);

            await viewModel.CreateCommand.ExecuteAsync(null);

            Assert.Equal(createdConversation.Id, raisedConversation?.Id);
        }

        [Fact]
        public async Task CreateCommand_ServiceThrows_SetsErrorMessage()
        {
            var viewModel = CreateViewModel();
            viewModel.GroupName = "Team";

            _groupServiceMock
                .Setup(service => service.CreateGroupAsync(_currentUserId, "Team", null, It.IsAny<List<Guid>>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            await viewModel.CreateCommand.ExecuteAsync(null);

            Assert.Equal("boom", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task CreateCommand_AfterExecution_StopsLoading()
        {
            var viewModel = CreateViewModel();
            viewModel.GroupName = "Team";
            _groupServiceMock
                .Setup(service => service.CreateGroupAsync(_currentUserId, "Team", null, It.IsAny<List<Guid>>()))
                .ReturnsAsync(new Conversation());

            await viewModel.CreateCommand.ExecuteAsync(null);

            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public async Task CancelCommand_RaisesCancelled()
        {
            var viewModel = CreateViewModel();
            var cancelled = false;
            viewModel.Cancelled += () => cancelled = true;

            await viewModel.CancelCommand.ExecuteAsync(null);

            Assert.True(cancelled);
        }
        
        [Fact]
        public async Task MemberSearchQuery_WithOnlyExcludedUsers_ResultsStayEmpty()
        {
            var selectedUser = new User { Id = Guid.NewGuid(), Username = "selected" };
            var viewModel = CreateViewModel();
            viewModel.SelectedMembers.Add(selectedUser);

            _searchServiceMock
                .Setup(service => service.SearchUsersAsync("john"))
                .ReturnsAsync(new List<User>
                {
                    new() { Id = _currentUserId, Username = "me" },
                    selectedUser,
                });

            viewModel.MemberSearchQuery = "john";
            await Task.Delay(20);

            Assert.Empty(viewModel.MemberSearchResults);
        }

        [Fact]
        public async Task CreateCommand_Success_WithoutSubscriber_DoesNotThrow_AndStopsLoading()
        {
            var viewModel = CreateViewModel();
            viewModel.GroupName = "Team";

            _groupServiceMock
                .Setup(service => service.CreateGroupAsync(
                    _currentUserId,
                    "Team",
                    null,
                    It.Is<List<Guid>>(ids => ids.Count == 0)))
                .ReturnsAsync(new Conversation { Id = Guid.NewGuid(), Title = "Team" });

            await viewModel.CreateCommand.ExecuteAsync(null);

            Assert.False(viewModel.IsLoading);
            Assert.Null(viewModel.ErrorMessage);
        }

        [Fact]
        public async Task CancelCommand_WithoutSubscriber_DoesNotThrow()
        {
            var viewModel = CreateViewModel();

            var exception = await Record.ExceptionAsync(() => viewModel.CancelCommand.ExecuteAsync(null));

            Assert.Null(exception);
        }

        [Fact]
        public async Task CreateCommand_Success_PassesEmptyMemberList()
        {
            var viewModel = CreateViewModel();
            viewModel.GroupName = "EmptyGroup";

            _groupServiceMock
                .Setup(service => service.CreateGroupAsync(
                    _currentUserId,
                    "EmptyGroup",
                    null,
                    It.Is<List<Guid>>(ids => ids.Count == 0)))
                .ReturnsAsync(new Conversation { Id = Guid.NewGuid(), Title = "EmptyGroup" });

            await viewModel.CreateCommand.ExecuteAsync(null);

            _groupServiceMock.Verify(service => service.CreateGroupAsync(
                _currentUserId,
                "EmptyGroup",
                null,
                It.Is<List<Guid>>(ids => ids.Count == 0)),
                Times.Once);
        }

        [Fact]
        public void GroupName_SetAndGet_Works()
        {
            var viewModel = CreateViewModel();

            viewModel.GroupName = "My Group";

            Assert.Equal("My Group", viewModel.GroupName);
        }

        [Fact]
        public void IconUrl_SetAndGet_Works()
        {
            var viewModel = CreateViewModel();

            viewModel.IconUrl = "icon.png";

            Assert.Equal("icon.png", viewModel.IconUrl);
        }

        [Fact]
        public void ErrorMessage_SetAndGet_Works()
        {
            var viewModel = CreateViewModel();

            viewModel.ErrorMessage = "err";

            Assert.Equal("err", viewModel.ErrorMessage);
        }

        [Fact]
        public void IsLoading_SetAndGet_Works()
        {
            var viewModel = CreateViewModel();

            viewModel.IsLoading = true;

            Assert.True(viewModel.IsLoading);
        }
        
        [Fact]
        public async Task MemberSearchQuery_SameValueTwice_DoesNotTriggerNewSearch()
        {
            var viewModel = CreateViewModel();

            _searchServiceMock
                .Setup(service => service.SearchUsersAsync("john"))
                .ReturnsAsync(new List<User>());

            viewModel.MemberSearchQuery = "john";
            await Task.Delay(20);

            viewModel.MemberSearchQuery = "john";
            await Task.Delay(20);

            _searchServiceMock.Verify(service => service.SearchUsersAsync("john"), Times.Once);
        }
        
        
    }
}
