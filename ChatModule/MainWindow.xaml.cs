using BCrypt.Net;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.Database;
// --- MERGED TEAM NAMESPACES ---
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories;
using ChatAndEvents.Data.EventsData.Repositories.achievementRepository;
using ChatAndEvents.Data.EventsData.Repositories.announcementRepository;
using ChatAndEvents.Data.EventsData.Repositories.categoriesRepository;
using ChatAndEvents.Data.EventsData.Repositories.discussionRepository;
using ChatAndEvents.Data.EventsData.Repositories.eventRepository;
using ChatAndEvents.Data.EventsData.Repositories.eventStatisticsRepository;
using ChatAndEvents.Data.EventsData.Repositories.notificationRepository;
using ChatAndEvents.Data.EventsData.Repositories.reputationRepository;
using ChatAndEvents.Data.EventsData.Services;
using ChatAndEvents.Data.EventsData.Services.achievementServices;
using ChatAndEvents.Data.EventsData.Services.announcementServices;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.categoryServices;
using ChatAndEvents.Data.EventsData.Services.discussionService;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatModule.src.HttpService;
using ChatModule.src.view_models;
using ChatModule.src.views;
using ChatModule.ViewModels;
using Events_GSS.ViewModels;
using Events_GSS.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Net.Http; // Added for HttpClient
using System.Threading.Tasks;

namespace ChatModule
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }
        private readonly Guid _initialUserId;
        private readonly string _initialUsername;

        private readonly IUserRepository _userRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IConversationListService _conversationListService;
        private readonly IParticipantRepository _participantRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IDirectMessageService _directMessageService;
        private readonly IGroupService _groupService;
        private readonly ISearchService _searchService;
        private readonly IMessageService _messageService;
        private readonly IMessageInteractionService _messageInteractionService;
        private readonly IReadReceiptService _readReceiptService;
        private readonly IMentionService _mentionService;
        private readonly IFriendRequestService _friendRequestService;
        private readonly IBlockService _blockService;
        private readonly IProfileService _profileService;
        private readonly IMemberPanelService _memberPanelService;
        private readonly IModerationService _moderationService;

        private const string ConnectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ChatAndEventsDB;" +
            "Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        private static readonly Uri ApiBaseAddress = new("http://localhost:5572/");

        public MainWindow()
            : this(Guid.Empty, "guest")
        {
        }

        public MainWindow(Guid userId, string username)
        {
            _initialUserId = userId;
            _initialUsername = username;

            var services = new ServiceCollection();

            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(ConnectionString),
                ServiceLifetime.Transient);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(ConnectionString),
                ServiceLifetime.Transient);

            // --- REPOSITORIES ---
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IFriendRepository, FriendRepository>();
            services.AddTransient<IConversationRepository, ConversationRepository>();
            services.AddTransient<IParticipantRepository, ParticipantRepository>();
            services.AddTransient<IMessageRepository, MessageRepository>();

            // --- LOCAL CHAT SERVICES (Not yet migrated) ---

            // ==========================================================
            // --- THE BATCH SWITCH: NEW CLOUD HTTP SERVICES ---
            // ==========================================================
            var baseAddress = ApiBaseAddress;

            services.AddHttpClient<IMemberPanelService, MemberPanelHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IFriendRequestService, FriendRequestHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IConversationListService, ConversationListHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IFriendListService, FriendListHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IBlockService, BlockHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IDirectMessageService, DirectMessageHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IGroupService, GroupHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IMentionService, MentionHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IAnnouncementService, ChatAndEvents.Data.EventsData.Services.announcementServices.AnnouncementHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IAttendedEventService, ChatAndEvents.Data.EventsData.Services.attendedEventServices.AttendedEventHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IAchievementService, ChatAndEvents.Data.EventsData.Services.achievementServices.AchievementHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IUserService, ChatAndEvents.Data.EventsData.Services.userServices.UserHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<ISearchService, SearchHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IMessageInteractionService, MessageInteractionHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IReadReceiptService, ReadReceiptHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IProfileService, ProfileHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IModerationService, ModerationHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IMessageService, MessageHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            // --- OLD DATABASE SERVICES (Commented out) ---
            // services.AddTransient<FriendRequestService>();
            // services.AddTransient<FriendListService>();
            // services.AddTransient<BlockService>();
            // services.AddTransient<DirectMessageService>();
            // services.AddTransient<GroupService>();
            // ==========================================================


            // --- EVENTS/GSS REPOSITORIES ---
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<IQuestRepository, QuestRepository>();
            services.AddTransient<IQuestMemoryRepository, QuestMemoryRepository>();
            services.AddTransient<IAnnouncementRepository, AnnouncementRepository>();
            services.AddTransient<IDiscussionRepository, DiscussionRepository>();
            services.AddTransient<IMemoryRepository, MemoryRepository>();
            services.AddTransient<IAttendedEventRepository, AttendedEventRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<IReputationRepository, ReputationRepository>();
            services.AddTransient<IAchievementRepository, AchievementRepository>();
            services.AddTransient<IEventStatisticsRepository, EventStatisticsRepository>();
            services.AddSingleton(new ChatAndEvents.Data.EventsData.Services.userServices.CurrentUserContext(_initialUserId));

            // --- EVENTS/GSS HTTP SERVICES ---
            services.AddHttpClient<IEventService, EventHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<ICategoryServices, CategoryHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IQuestService, QuestHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IQuestApprovalService, QuestApprovalHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IDiscussionService, DiscussionHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IMemoryService, MemoryHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<INotificationService, NotificationHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IReputationService, ReputationHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddHttpClient<IEventStatisticsService, EventStatisticsHttpService>(client =>
            {
                client.BaseAddress = baseAddress;
            });
            // --- EVENTS/GSS SERVICES ---
            services.AddTransient<EventListingViewModel>();
            services.AddTransient<ReputationViewModel>();
            services.AddTransient<NotificationViewModel>();


            Events_GSS.App.Services = services.BuildServiceProvider();
            Events_GSS.App.MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);

            var provider = Events_GSS.App.Services;

            // --- RESOLVING WITH INTERFACES ---
            _userRepository = provider.GetRequiredService<IUserRepository>();
            _conversationRepository = provider.GetRequiredService<IConversationRepository>();
            _participantRepository = provider.GetRequiredService<IParticipantRepository>();
            _messageRepository = provider.GetRequiredService<IMessageRepository>();

            _directMessageService = provider.GetRequiredService<IDirectMessageService>();
            _groupService = provider.GetRequiredService<IGroupService>();
            _searchService = provider.GetRequiredService<ISearchService>();
            _messageService = provider.GetRequiredService<IMessageService>();
            _messageInteractionService = provider.GetRequiredService<IMessageInteractionService>();
            _readReceiptService = provider.GetRequiredService<IReadReceiptService>();
            _mentionService = provider.GetRequiredService<IMentionService>();
            _friendRequestService = provider.GetRequiredService<IFriendRequestService>();
            _blockService = provider.GetRequiredService<IBlockService>();
            _profileService = provider.GetRequiredService<IProfileService>();
            _memberPanelService = provider.GetRequiredService<IMemberPanelService>();
            _moderationService = provider.GetRequiredService<IModerationService>();

            // 1. Get the newly extracted Interface!
            _conversationListService = provider.GetRequiredService<IConversationListService>();
            var friendListService = provider.GetRequiredService<IFriendListService>();

            ViewModel = new MainViewModel(
                _conversationListService,
                _friendRequestService,
                friendListService,
                _blockService,
                _profileService,
                _directMessageService,
                // --- GSS SERVICES ---
                // (Notice that IEventRepository is completely GONE from this list!)
                provider.GetRequiredService<INotificationService>(),
                provider.GetRequiredService<IReputationService>(),
                provider.GetRequiredService<IUserService>(),
                provider.GetRequiredService<IEventService>(),
                provider.GetRequiredService<IQuestService>(),
                provider.GetRequiredService<IAttendedEventService>(),
                provider.GetRequiredService<IAchievementService>());

            InitializeComponent();

            ViewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.CurrentPage))
                {
                    SafeRenderCurrentPage();
                }
            };
            ViewModel.NavigateToChatRequested += conversationId => _ = OpenChatAsync(conversationId);

            ViewModel.NavigateToLoginRequested += () =>
            {

                var loginServices = new ServiceCollection();
                loginServices.AddHttpClient<IAuthenticationService, AuthenticationHttpService>(client =>
                {
                    client.BaseAddress = baseAddress;
                });
                var loginProvider = loginServices.BuildServiceProvider();

                var loginWindow = new LoginWindow(loginProvider.GetRequiredService<IAuthenticationService>());
                loginWindow.LoginSucceeded += (newUserId, newUsername) =>
                {
                    var nextMain = new MainWindow(newUserId, newUsername);
                    App.SetMainWindow(nextMain);
                    nextMain.Activate();
                    loginWindow.Close();
                    Close();
                    return Task.CompletedTask;
                };
                loginWindow.Activate();
            };

            _ = InitialiseAndRenderAsync();
        }


        private async System.Threading.Tasks.Task InitialiseAndRenderAsync()
        {
            try
            {
                await ViewModel.InitialiseAsync(_initialUserId, _initialUsername);
                SafeRenderCurrentPage();
            }
            catch (Exception ex)
            {
                if (CurrentPageHost.XamlRoot != null)
                {
                    await ShowInfoDialogAsync("Startup error", ex.Message);
                }
            }
        }

        private void RenderCurrentPage()
        {
            object? view = ViewModel.CurrentPage switch
            {
                ConversationListViewModel vm => BuildConversationListView(vm),
                FriendListViewModel vm => new FriendListView(vm),
                FriendRequestsViewModel vm => new FriendRequestsView(vm),
                ProfileViewModel vm => BuildProfileView(vm),
                ChatViewModel vm => new ChatView(vm),

                EventListingViewModel vm => new EventListingPage(vm),
                ReputationViewModel vm => new ReputationPage(vm),
                NotificationViewModel vm => new NotificationView(vm),
                CreateEventViewModel vm => new CreateEventPage(),
                EventDetailViewModel vm => new EventDetailPage(vm),
                EventStatisticsViewModel vm => new EventStatisticsPage(vm),
                MyEventsViewModel vm => new MyEventsPage(vm),

                _ => null
            };

            CurrentPageHost.Content = view;
        }

        private void SafeRenderCurrentPage()
        {
            try
            {
                RenderCurrentPage();
            }
            catch (Exception ex)
            {
                CurrentPageHost.Content = new TextBlock
                {
                    Text = $"Failed to render page: {ex.Message}",
                    Margin = new Thickness(16)
                };
            }
        }

        private ConversationListView BuildConversationListView(ConversationListViewModel vm)
        {
            vm.NewGroupRequested -= OnNewGroupRequested;
            vm.NewDmRequested -= OnNewDmRequested;
            vm.ConversationOpened -= OnConversationOpened;

            vm.NewGroupRequested += OnNewGroupRequested;
            vm.NewDmRequested += OnNewDmRequested;
            vm.ConversationOpened += OnConversationOpened;

            return new ConversationListView(vm);
        }

        private ProfileView BuildProfileView(ProfileViewModel vm)
        {
            vm.NavigateToChatRequested -= OnConversationOpened;
            vm.NavigateToChatRequested += OnConversationOpened;
            return new ProfileView(vm);
        }

        private void OnConversationOpened(Guid conversationId) => _ = OpenChatAsync(conversationId);
        private void OnNewGroupRequested() => _ = ShowCreateGroupDialogAsync();
        private void OnNewDmRequested() => _ = ShowCreateDmDialogAsync();

        private async Task ShowCreateGroupDialogAsync()
        {
            var createGroupViewModel = new CreateGroupViewModel(_groupService, _searchService, ViewModel.CurrentUserId);
            var dialog = new CreateGroupDialog(createGroupViewModel)
            {
                XamlRoot = CurrentPageHost.XamlRoot
            };

            _ = await dialog.ShowAsync();

            if (dialog.CreatedConversation != null)
            {
                await OpenChatAsync(dialog.CreatedConversation.Id);
            }
        }

        private async Task ShowCreateDmDialogAsync()
        {
            if (CurrentPageHost.XamlRoot == null) return;

            var usernameBox = new TextBox
            {
                PlaceholderText = "Enter username",
                Margin = new Thickness(0, 8, 0, 0)
            };

            var dialog = new ContentDialog
            {
                Title = "Start New DM",
                Content = usernameBox,
                PrimaryButtonText = "Start",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = CurrentPageHost.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            var username = usernameBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(username)) return;

            var user = (await _searchService.SearchUsersAsync(username))
                .FirstOrDefault(candidate => string.Equals(candidate.Username, username, StringComparison.OrdinalIgnoreCase));
            if (user == null || user.Id == ViewModel.CurrentUserId)
            {
                await ShowInfoDialogAsync("User not found", "Enter another username to start a DM.");
                return;
            }

            var conversation = await _directMessageService.GetOrCreateAsync(ViewModel.CurrentUserId, user.Id);
            await OpenChatAsync(conversation.Id);
        }

        private async Task ShowInfoDialogAsync(string title, string message)
        {
            if (CurrentPageHost.XamlRoot == null) return;

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = CurrentPageHost.XamlRoot
            };

            _ = await dialog.ShowAsync();
        }

        private async Task<string?> ShowInputDialogAsync(string title, string placeholder)
        {
            var inputBox = new TextBox
            {
                PlaceholderText = placeholder,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = inputBox,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = CurrentPageHost.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return null;

            return inputBox.Text;
        }

        private async Task OpenChatAsync(Guid conversationId)
        {
            try
            {
                var conversation = await _conversationListService.GetByIdAsync(conversationId);
                if (conversation == null) return;

                var chatViewModel = new ChatViewModel(
                    _messageService,
                    _messageInteractionService,
                    _readReceiptService,
                    _mentionService,
                    _directMessageService,
                    _conversationListService,
                    _searchService,
                    ViewModel.CurrentUserId);

                await chatViewModel.LoadAsync(conversationId);

                var chatView = new ChatView(chatViewModel);

                chatViewModel.LeaveGroupRequested += async () =>
                {
                    try
                    {
                        await _groupService.LeaveGroupAsync(conversationId, ViewModel.CurrentUserId);
                        await ShowInfoDialogAsync("Group", "You left the group.");
                        ViewModel.GoToConversationsCommand.Execute(null);
                    }
                    catch (Exception ex)
                    {
                        await ShowInfoDialogAsync("Unable to leave group", ex.Message);
                    }
                };

                chatViewModel.SetNicknameRequested += async () =>
                {
                    var nickname = await ShowInputDialogAsync("Set group nickname", "Nickname (max 16 chars)");
                    if (nickname == null) return;

                    try
                    {
                        await _messageService.SetNicknameAsync(conversationId, ViewModel.CurrentUserId, nickname);
                        await chatViewModel.LoadAsync(conversationId);
                    }
                    catch (Exception ex)
                    {
                        await ShowInfoDialogAsync("Nickname", ex.Message);
                    }
                };

                chatViewModel.ClearNicknameRequested += async () =>
                {
                    try
                    {
                        await _messageService.SetNicknameAsync(conversationId, ViewModel.CurrentUserId, null);
                        await chatViewModel.LoadAsync(conversationId);
                    }
                    catch (Exception ex)
                    {
                        await ShowInfoDialogAsync("Nickname", ex.Message);
                    }
                };

                if (conversation.Type == ConversationType.Group)
                {
                    var memberPanelViewModel = new MemberPanelViewModel(_memberPanelService, _moderationService, ViewModel.CurrentUserId);
                    memberPanelViewModel.NavigateToProfileRequested += async userId =>
                    {
                        var profileVm = new ProfileViewModel(_friendRequestService, _blockService, _directMessageService, _profileService, ViewModel.CurrentUserId);
                        await profileVm.LoadAsync(userId);
                        var profilePanelVm = new ConversationSidePanelViewModel(ConversationType.Dm, profileVm, () =>
                        {
                            var membersPanelVm = new ConversationSidePanelViewModel(ConversationType.Group, memberPanelViewModel);
                            chatView.SetSidePanel(new ConversationSidePanelView(membersPanelVm));
                        });
                        chatView.SetSidePanel(new ConversationSidePanelView(profilePanelVm));
                    };
                    await memberPanelViewModel.LoadAsync(conversationId);
                    var sideVm = new ConversationSidePanelViewModel(ConversationType.Group, memberPanelViewModel);
                    chatView.SetSidePanel(new ConversationSidePanelView(sideVm));
                }
                else
                {
                    var otherUser = await _directMessageService.GetOtherUserAsync(conversationId, ViewModel.CurrentUserId);
                    if (otherUser != null)
                    {
                        var profileVm = new ProfileViewModel(_friendRequestService, _blockService, _directMessageService, _profileService, ViewModel.CurrentUserId);
                        await profileVm.LoadAsync(otherUser.Id);
                        var sideVm = new ConversationSidePanelViewModel(ConversationType.Dm, profileVm);
                        chatView.SetSidePanel(new ConversationSidePanelView(sideVm));
                    }
                }

                CurrentPageHost.Content = chatView;
            }
            catch (InvalidOperationException ex)
            {
                await ShowInfoDialogAsync("Unable to open conversation", ex.Message);
                ViewModel.GoToConversationsCommand.Execute(null);
            }
        }
    }
}
