using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatModule.ViewModels;

namespace ChatModule.src.view_models
{
    public class FriendRequestsViewModel : BaseViewModel
    {
        private readonly IFriendRequestService _friendRequestService;
        private readonly Guid _currentUserId;

        public ObservableCollection<User> IncomingRequests { get; } = new ();

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => Set(ref isLoading, value);
        }

        public event Action? NavigateBackRequested;

        public RelayCommand LoadCommand { get; }
        public RelayCommand<Guid> AcceptCommand { get; }
        public RelayCommand<Guid> DeclineCommand { get; }
        public RelayCommand BackCommand { get; }

        public FriendRequestsViewModel(IFriendRequestService friendRequestService, Guid currentUserId)
        {
            this._friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
            this._currentUserId = currentUserId;

            LoadCommand = new RelayCommand(LoadIncomingRequestsAsync);
            AcceptCommand = new RelayCommand<Guid>(AcceptFriendRequestAsync);
            DeclineCommand = new RelayCommand<Guid>(DeclineFriendRequestAsync);
            BackCommand = new RelayCommand(NavigateBackAsync);
        }

        private async Task LoadIncomingRequestsAsync()
        {
            IsLoading = true;
            try
            {
                var pendingRequestsList = await _friendRequestService.GetIncomingRequestsAsync(_currentUserId);
                IncomingRequests.Clear();
                foreach (var requesterUser in pendingRequestsList)
                {
                    IncomingRequests.Add(requesterUser);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AcceptFriendRequestAsync(Guid requesterUserId)
        {
            await _friendRequestService.AcceptFriendRequestAsync(_currentUserId, requesterUserId);
            await LoadIncomingRequestsAsync();
        }

        private async Task DeclineFriendRequestAsync(Guid requesterUserId)
        {
            await _friendRequestService.DeclineFriendRequestAsync(_currentUserId, requesterUserId);
            await LoadIncomingRequestsAsync();
        }

        private Task NavigateBackAsync()
        {
            NavigateBackRequested?.Invoke();
            return Task.CompletedTask;
        }
    }
}
