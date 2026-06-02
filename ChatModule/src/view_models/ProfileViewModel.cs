using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;

namespace ChatModule.ViewModels;
public class ProfileViewModel : BaseViewModel
{
    private readonly IFriendRequestService _friendRequestService;
    private readonly IBlockService _blockService;
    private readonly IDirectMessageService _directMessageService;
    private readonly Guid _currentUserId;
    private readonly IProfileService _profileService;
    private bool suppressStatusUpdate;

    private User? user;
    public User? User
    {
        get => user;
        set
        {
            if (Set(ref user, value))
            {
                OnPropertyChanged(nameof(StatusBadgeText));
            }
        }
    }

    private bool isBlocked;
    public bool IsBlocked
    {
        get => isBlocked;
        set => Set(ref isBlocked, value);
    }

    private bool isOwnProfile;
    public bool IsOwnProfile
    {
        get => isOwnProfile;
        set => Set(ref isOwnProfile, value);
    }

    public ObservableCollection<User> MutualFriends { get; } = new ();

    private bool areMutualFriendsVisible;
    public bool AreMutualFriendsVisible
    {
        get => areMutualFriendsVisible;
        set => Set(ref areMutualFriendsVisible, value);
    }

    public event Action<Guid>? NavigateToChatRequested;

    public RelayCommand SendFriendRequestCommand { get; }
    public RelayCommand BlockUserCommand { get; }
    public RelayCommand UnblockUserCommand { get; }
    public RelayCommand OpenDmCommand { get; }

    private UserStatus selectedStatus;
    public UserStatus SelectedStatus
    {
        get => selectedStatus;
        set
        {
            if (Set(ref selectedStatus, value))
            {
                OnPropertyChanged(nameof(StatusBadgeText));

                if (!suppressStatusUpdate && User != null && IsOwnProfile)
                {
                    _ = UpdateStatusAsync(value);
                }
            }
        }
    }

    public string StatusBadgeText => SelectedStatus.ToString();

    private string? editBio;
    public string? EditBio
    {
        get => editBio;
        set
        {
            if (Set(ref editBio, value))
            {
                OnPropertyChanged(nameof(DisplayBio));
            }
        }
    }

    public string DisplayBio => string.IsNullOrWhiteSpace(EditBio) ? "No bio yet" : EditBio!;

    private string? editAvatarUrl;
    public string? EditAvatarUrl
    {
        get => editAvatarUrl;
        set => Set(ref editAvatarUrl, value);
    }

    private DateTime? editBirthday;
    public DateTime? EditBirthday
    {
        get => editBirthday;
        set
        {
            if (Set(ref editBirthday, value))
            {
                OnPropertyChanged(nameof(EditBirthdayOffset));
                OnPropertyChanged(nameof(IsBirthdayToday));
                OnPropertyChanged(nameof(BirthdayText));
            }
        }
    }

    public string BirthdayText => EditBirthday.HasValue
        ? EditBirthday.Value.ToString("MMMM d, yyyy")
        : "Birthday not set";

    public int MutualFriendsCount => MutualFriends.Count;

    public string MutualFriendsLabel => $"Mutual Friends ({MutualFriendsCount})";

    public RelayCommand SaveProfileCommand { get; }
    public RelayCommand LoadMutualFriendsCommand { get; }
    public ObservableCollection<UserStatus> AvailableStatuses { get; } =
    [
        UserStatus.Online,
        UserStatus.Offline,
        UserStatus.Busy
    ];

    public DateTimeOffset EditBirthdayOffset
    {
        get => EditBirthday.HasValue
            ? new DateTimeOffset(EditBirthday.Value.Date)
            : DateTimeOffset.Now;
        set
        {
            EditBirthday = value.DateTime.Date;
            OnPropertyChanged(nameof(IsBirthdayToday));
        }
    }

    public bool IsBirthdayToday
    {
        get
        {
            if (!EditBirthday.HasValue)
            {
                return false;
            }

            var today = DateTime.Today;
            return EditBirthday.Value.Month == today.Month && EditBirthday.Value.Day == today.Day;
        }
    }

    public ProfileViewModel(
        IFriendRequestService friendRequestService,
        IBlockService blockService,
        IDirectMessageService directMessageService,
        IProfileService profileService,
        Guid currentUserId)
    {
        this._friendRequestService = friendRequestService ?? throw new ArgumentNullException(nameof(friendRequestService));
        this._blockService = blockService ?? throw new ArgumentNullException(nameof(blockService));
        this._directMessageService = directMessageService ?? throw new ArgumentNullException(nameof(directMessageService));
        this._currentUserId = currentUserId;
        this._profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));

        MutualFriends.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(MutualFriendsCount));
            OnPropertyChanged(nameof(MutualFriendsLabel));
        };

        SendFriendRequestCommand = new RelayCommand(SendFriendRequestAsync);
        BlockUserCommand = new RelayCommand(BlockUserAsync);
        UnblockUserCommand = new RelayCommand(UnblockUserAsync);
        OpenDmCommand = new RelayCommand(OpenDmAsync);

        SaveProfileCommand = new RelayCommand(SaveProfileAsync);
        LoadMutualFriendsCommand = new RelayCommand(LoadMutualFriendsAsync);
    }

    public async Task LoadAsync(Guid targetUserId)
    {
        User = await _profileService.GetProfileAsync(targetUserId);
        IsOwnProfile = targetUserId == _currentUserId;

        suppressStatusUpdate = true;

        try
        {
            if (User != null)
            {
                SelectedStatus = User.Status;
                EditBio = User.Bio;
                EditAvatarUrl = User.AvatarUrl;
                EditBirthday = User.Birthday;
            }
        }
        finally
        {
            suppressStatusUpdate = false;
        }

        AreMutualFriendsVisible = false;

        if (!IsOwnProfile && User != null)
        {
            IsBlocked = await _blockService.IsBlockedAsync(_currentUserId, User.Id);
            await RefreshMutualFriendsAsync();
        }
        else
        {
            IsBlocked = false;
            MutualFriends.Clear();
        }
    }

    private async Task UpdateStatusAsync(UserStatus status)
    {
        if (User == null)
        {
            return;
        }

        await _profileService.UpdateStatusAsync(User.Id, status);
    }

    private async Task SaveProfileAsync()
    {
        if (User == null)
        {
            return;
        }

        await _profileService.UpdateProfileAsync(User.Id, EditBio, EditAvatarUrl, EditBirthday);
        User = await _profileService.GetProfileAsync(User.Id);

        if (User != null)
        {
            suppressStatusUpdate = true;
            try
            {
                SelectedStatus = User.Status;
                EditBio = User.Bio;
                EditAvatarUrl = User.AvatarUrl;
                EditBirthday = User.Birthday;
            }
            finally
            {
                suppressStatusUpdate = false;
            }
        }
    }

    private async Task LoadMutualFriendsAsync()
    {
        if (IsOwnProfile || User == null)
        {
            return;
        }

        if (!AreMutualFriendsVisible || MutualFriends.Count == 0)
        {
            await RefreshMutualFriendsAsync();
        }

        AreMutualFriendsVisible = !AreMutualFriendsVisible;
    }

    private async Task RefreshMutualFriendsAsync()
    {
        MutualFriends.Clear();
        if (IsOwnProfile || User == null)
        {
            return;
        }

        var mutuals = await _profileService.GetMutualFriendsAsync(_currentUserId, User.Id);
        foreach (var user in mutuals)
        {
            MutualFriends.Add(user);
        }
    }

    private async Task SendFriendRequestAsync()
    {
        if (User == null)
        {
            return;
        }

        if (IsBlocked)
        {
            return;
        }

        var existingRelationshipStatus = await _friendRequestService.GetRelationshipStatusAsync(_currentUserId, User.Id);
        if (existingRelationshipStatus.HasValue)
        {
            return;
        }

        await _friendRequestService.SendFriendRequestAsync(_currentUserId, User.Id);
    }

    private async Task BlockUserAsync()
    {
        if (User == null)
        {
            return;
        }

        await _blockService.BlockUserAsync(_currentUserId, User.Id);
        IsBlocked = true;
    }

    private async Task UnblockUserAsync()
    {
        if (User == null)
        {
            return;
        }

        await _blockService.UnblockUserAsync(_currentUserId, User.Id);
        IsBlocked = false;
    }

    private async Task OpenDmAsync()
    {
        if (User == null)
        {
            return;
        }

        var conversation = await _directMessageService.GetOrCreateAsync(_currentUserId, User.Id);
        NavigateToChatRequested?.Invoke(conversation.Id);
    }
}
