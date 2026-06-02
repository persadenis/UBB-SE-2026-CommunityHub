using ChatModule.src.view_models;
using ChatAndEvents.Data.EventsData.Repositories.notificationRepository;
using ChatAndEvents.Data.EventsData.Repositories.reputationRepository;
using Events_GSS.ViewModels;
using System;
using System.Threading.Tasks;
using Events_GSS.Views;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories.eventRepository;
using ChatAndEvents.Data.EventsData.Services.achievementServices;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services; // Added for the Chat Interfaces!
using Microsoft.Extensions.DependencyInjection;

namespace ChatModule.ViewModels
{
    public class MainViewModel : BaseViewModel    
    {
        // --- Chat Services (Now strictly using Interfaces!) ---
        private readonly IConversationListService _conversationListService;
        private readonly IFriendRequestService _friendRequestService;
        private readonly IFriendListService? _friendListService;
        private readonly IBlockService _blockService;
        private readonly IDirectMessageService _directMessageService;
        private readonly IProfileService _profileService;

        // --- Event/GSS Services ---
        // (Removed the illegal IEventRepository from here!)
        private readonly INotificationService _notificationService;
        private readonly IReputationService _reputationService;
        private readonly IUserService _userService;

        // Needed specifically for creating an event
        private readonly IEventService _eventService;
        private readonly IQuestService _questService;
        private readonly IAchievementService _achievementService;
        private readonly IAttendedEventService _attendedEventService;

        private Guid _currentUserId;
        public Guid CurrentUserId
        {
            get => _currentUserId;
            private set => Set(ref _currentUserId, value);
        }

        private string _currentUsername = string.Empty;
        public string CurrentUsername
        {
            get => _currentUsername;
            private set => Set(ref _currentUsername, value);
        }

        private object? _currentPage;
        public object? CurrentPage
        {
            get => _currentPage;
            private set => Set(ref _currentPage, value);
        }

        public event Action? NavigateToLoginRequested;
        public event Action<Guid>? NavigateToChatRequested;

        // --- ORIGINAL COMMANDS ---
        public RelayCommand GoToConversationsCommand { get; }
        public RelayCommand GoToFriendsCommand { get; }
        public RelayCommand GoToProfileCommand { get; }
        public RelayCommand LogoutCommand { get; }

        // --- NEW MERGED COMMANDS ---
        public RelayCommand GoToEventsCommand { get; }
        public RelayCommand GoToMyEventsCommand { get; }
        public RelayCommand GoToReputationCommand { get; }
        public RelayCommand GoToNotificationsCommand { get; }
        public RelayCommand GoToCreateEventCommand { get; }

        // Constructor 1 (Without FriendListService)
        public MainViewModel(
            IConversationListService conversationListService,
            IFriendRequestService friendRequestService,
            IBlockService blockService,
            IProfileService profileService,
            IDirectMessageService directMessageService,
            // --- GSS SERVICES ---
            INotificationService notificationService,
            IReputationService reputationService,
            IUserService userService,
            IEventService eventService,
            IQuestService questService,
            IAttendedEventService attendedEventService,
            IAchievementService achievementService)
            : this(
                conversationListService,
                friendRequestService,
                null, // friendListService
                blockService,
                profileService,
                directMessageService,
                notificationService,
                reputationService,
                userService,
                eventService,
                questService,
                attendedEventService,
                achievementService)
        {
        }

        // Constructor 2 (The Main One)
        public MainViewModel(
            IConversationListService conversationListService,
            IFriendRequestService friendRequestService,
            IFriendListService? friendListService,
            IBlockService blockService,
            IProfileService profileService,
            IDirectMessageService directMessageService,
            // --- GSS SERVICES ---
            INotificationService notificationService,
            IReputationService reputationService,
            IUserService userService,
            IEventService eventService,
            IQuestService questService,
            IAttendedEventService attendedEventService,
            IAchievementService achievementService)
        {
            _conversationListService = conversationListService ?? throw new ArgumentNullException(nameof(conversationListService));
            _friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
            _friendListService = friendListService;
            _blockService = blockService ?? throw new ArgumentNullException(nameof(blockService));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _directMessageService = directMessageService ?? throw new ArgumentNullException(nameof(directMessageService));

            // Assign the Events/GSS services
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _reputationService = reputationService ?? throw new ArgumentNullException(nameof(reputationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _questService = questService ?? throw new ArgumentNullException(nameof(questService));
            _attendedEventService = attendedEventService ?? throw new ArgumentNullException(nameof(attendedEventService));
            _achievementService = achievementService ?? throw new ArgumentNullException(nameof(achievementService));

            // Initialize Original Commands
            GoToConversationsCommand = new RelayCommand(GoToConversationsAsync);
            GoToFriendsCommand = new RelayCommand(GoToFriendsAsync);
            GoToProfileCommand = new RelayCommand(GoToProfileAsync);
            LogoutCommand = new RelayCommand(LogoutAsync);

            // Initialize New Merged Commands
            GoToEventsCommand = new RelayCommand(GoToEventsAsync);
            GoToMyEventsCommand = new RelayCommand(GoToMyEventsAsync);
            GoToReputationCommand = new RelayCommand(GoToReputationAsync);
            GoToNotificationsCommand = new RelayCommand(GoToNotificationsAsync);
            GoToCreateEventCommand = new RelayCommand(GoToCreateEventAsync);
        }

        public Task InitialiseAsync(Guid userId, string username)
        {
            CurrentUserId = userId;
            CurrentUsername = username;
            CurrentPage = new ConversationListViewModel(_conversationListService, CurrentUserId);
            return Task.CompletedTask;
        }

        private Task GoToConversationsAsync()
        {
            CurrentPage = new ConversationListViewModel(_conversationListService, CurrentUserId);
            return Task.CompletedTask;
        }

        private Task GoToFriendsAsync()
        {
            if (_friendListService != null)
            {
                var friendListViewModel = new FriendListViewModel(_friendListService, _friendRequestService, _directMessageService, CurrentUserId);
                friendListViewModel.NavigateToProfileRequested += OnNavigateToProfileFromFriends;
                friendListViewModel.NavigateToChatRequested += OnNavigateToChatFromFriends;
                friendListViewModel.OpenRequestsRequested += OnOpenRequestsFromFriends;
                CurrentPage = friendListViewModel;
                return Task.CompletedTask;
            }

            CurrentPage = new FriendRequestsViewModel(_friendRequestService, CurrentUserId);
            return Task.CompletedTask;
        }

        private Task GoToProfileAsync()
            => ShowProfileAsync(CurrentUserId);

        private async Task GoToEventsAsync()
        {
            // Now correctly passing the Service instead of the Repository!
            var vm = new EventListingViewModel(_eventService);

            // Listen for the "Create" shout, and execute the existing command if allowed
            vm.CreateEventRequested += () =>
            {
                if (GoToCreateEventCommand.CanExecute(null))
                {
                    GoToCreateEventCommand.Execute(null);
                }
            };

            // This is the "Listener" that connects the two.
            vm.EventDetailsRequested += (selectedEvent) =>
            {
                _ = GoToEventDetailsAsync(selectedEvent); 
            };

            await vm.LoadEventsAsync();
            CurrentPage = vm;
        }

        private async Task GoToMyEventsAsync()
        {
            var vm = new MyEventsViewModel(_eventService, _userService, _attendedEventService);

            vm.EventDetailsRequested += (selectedEvent) =>
            {
                _ = GoToMyEventDetailsAsync(selectedEvent);
            };

            await vm.LoadMyEventsAsync();
            CurrentPage = vm;
        }

        private Task GoToMyEventDetailsAsync(Event selectedEvent)
        {
            var vm = new EventDetailViewModel(selectedEvent, this._attendedEventService);

            // Back from detail goes back to My Events, not the public list
            vm.BackRequested += () => _ = this.GoToMyEventsAsync();
            vm.StatisticsRequested += selected => this.ShowStatistics(selected, () => _ = this.GoToMyEventDetailsAsync(selected));
            this.CurrentPage = vm;
            return Task.CompletedTask;
        }

        private async Task GoToReputationAsync()
        {
            var vm = new ReputationViewModel(_userService, _reputationService, _achievementService);
            await vm.LoadAsync();
            CurrentPage = vm;
        }

        private async Task GoToNotificationsAsync()
        {
            var vm = new NotificationViewModel(_notificationService, _userService);
            await vm.LoadAsync();

            CurrentPage = vm;
        }

        private async Task GoToCreateEventAsync()
        {
            try
            {
                // 1. Create the ViewModel with all required services
                var vm = new CreateEventViewModel(
                    _userService,
                    _eventService,
                    _questService,
                    _attendedEventService);

                // 2. Load the preset quests so Step 2/3 isn't empty
                await vm.LoadPresetQuestsAsync();

                // 3. Set up the event handler for when the user clicks "Cancel" or finishes creating
                vm.CloseRequested += (createdEventDto) =>
                {
                    // Whether they saved or canceled, take them back to the Events list
                    _ = GoToEventsAsync();
                };

                // 4. Show the page
                CurrentPage = vm;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Create Event page: {ex.Message}");
            }
        }

        private void OnNavigateToProfileFromFriends(Guid userId)
        {
            _ = ShowProfileAsync(userId);
        }

        private void OnNavigateToChatFromFriends(Guid conversationId)
        {
            NavigateToChatRequested?.Invoke(conversationId);
        }

        private void OnOpenRequestsFromFriends()
        {
            var friendRequestsViewModel = new FriendRequestsViewModel(_friendRequestService, CurrentUserId);
            friendRequestsViewModel.NavigateBackRequested += () => _ = GoToFriendsAsync();
            CurrentPage = friendRequestsViewModel;
        }

        private async Task ShowProfileAsync(Guid targetUserId)
        {
            var profileViewModel = new ProfileViewModel(_friendRequestService, _blockService, _directMessageService, _profileService, CurrentUserId);
            await profileViewModel.LoadAsync(targetUserId);
            CurrentPage = profileViewModel;
        }

        private Task GoToEventDetailsAsync(Event selectedEvent)
        {
            // Pass the service we already have in MainViewModel
            var vm = new EventDetailViewModel(selectedEvent, _attendedEventService);

            vm.BackRequested += () => _ = GoToEventsAsync();
            vm.StatisticsRequested += selected => this.ShowStatistics(selected, () => _ = this.GoToEventDetailsAsync(selected));
            CurrentPage = vm;
            return Task.CompletedTask;
        }

        private void ShowStatistics(Event selectedEvent, Action goBack)
        {
            var statisticsService = Events_GSS.App.Services.GetRequiredService<IEventStatisticsService>();
            var vm = new EventStatisticsViewModel(statisticsService, selectedEvent);
            vm.BackRequested += goBack;
            CurrentPage = vm;
        }

        private Task LogoutAsync()
        {
            if (CurrentUserId != Guid.Empty)
            {
                _ = _profileService.UpdateStatusAsync(CurrentUserId, UserStatus.Offline);
            }

            CurrentUserId = Guid.Empty;
            CurrentUsername = string.Empty;
            CurrentPage = null;
            NavigateToLoginRequested?.Invoke();
            return Task.CompletedTask;
        }
    }
}
