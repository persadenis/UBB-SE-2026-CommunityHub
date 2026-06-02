using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChatModule.ViewModels;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;

namespace ChatModule.src.view_models
{
    public class CreateGroupViewModel : BaseViewModel
    {
        private readonly IGroupService _groupService;
        private readonly ISearchService _searchService;
        private readonly Guid _currentUserId;
        
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public ObservableCollection<User> SelectedMembers { get; } = new ();
        public ObservableCollection<User> MemberSearchResults { get; } = new ();

        private string _groupName = string.Empty;
        public string GroupName
        {
            get => _groupName;
            set => Set(ref _groupName, value);
        }

        private string? _iconUrl;
        public string? IconUrl
        {
            get => _iconUrl;
            set => Set(ref _iconUrl, value);
        }

        private string _memberSearchQuery = string.Empty;
        public string MemberSearchQuery
        {
            get => _memberSearchQuery;
            set
            {
                if (Set(ref _memberSearchQuery, value))
                    _ = SearchMembersAsync();
            }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public RelayCommand CreateCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand<User> AddMemberCommand { get; }
        public RelayCommand<User> RemoveMemberCommand { get; }

        public event Action<Conversation>? GroupCreated;
        public event Action? Cancelled;

        public CreateGroupViewModel(
            IGroupService groupService,
            ISearchService searchService,
            Guid currentUserId)
        {
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _currentUserId = currentUserId;

            CreateCommand = new RelayCommand(CreateGroupAsync);
            CancelCommand = new RelayCommand(CancelAsync);
            AddMemberCommand = new RelayCommand<User>(AddMemberAsync);
            RemoveMemberCommand = new RelayCommand<User>(RemoveMemberAsync);
        }

        private async Task SearchMembersAsync()
        {
            MemberSearchResults.Clear();

            if (string.IsNullOrWhiteSpace(_memberSearchQuery))
            {
                return;
            }

            var selectedUserIds = SelectedMembers.Select(user => user.Id).ToHashSet();
            var users = await _searchService.SearchUsersAsync(_memberSearchQuery);

            foreach (var user in users.Where(user => user.Id != _currentUserId && !selectedUserIds.Contains(user.Id)))
            {
                MemberSearchResults.Add(user);
            }
        }

        private Task AddMemberAsync(User user)
        {
            if (SelectedMembers.All(u => u.Id != user.Id))
                SelectedMembers.Add(user);

            MemberSearchResults.Remove(user);
            return Task.CompletedTask;
        }

        private Task RemoveMemberAsync(User user)
        {
            SelectedMembers.Remove(user);
            return Task.CompletedTask;
        }

        private async Task CreateGroupAsync()
        {
            ErrorMessage = null;
            IsLoading = true;

            try
            {
                var memberIds = SelectedMembers.Select(user => user.Id).ToList();
                var conversation = await _groupService.CreateGroupAsync(_currentUserId, _groupName, _iconUrl, memberIds);
                GroupCreated?.Invoke(conversation);
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Task CancelAsync()
        {
            Cancelled?.Invoke();
            return Task.CompletedTask;
        }
    }
}
