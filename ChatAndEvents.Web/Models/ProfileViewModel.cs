using System;
using System.Collections.Generic;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Web.Models;

public class ProfileViewModel
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string AvatarInitial { get; set; } = "?";

    public string? Bio { get; set; }

    public UserStatus Status { get; set; }

    public DateTime? Birthday { get; set; }

    public bool IsOwnProfile { get; set; }

    public bool IsBlocked { get; set; }

    public FriendStatus? RelationshipStatus { get; set; }

    public List<FriendListItemViewModel> MutualFriends { get; set; } = new();

    public List<UserStatus> AvailableStatuses { get; set; } =
    [
        UserStatus.Online,
        UserStatus.Offline,
        UserStatus.Busy,
    ];

    public string DisplayBio => string.IsNullOrWhiteSpace(Bio) ? "No bio yet" : Bio;

    public string StatusLabel => Status.ToString();

    public string BirthdayText => Birthday.HasValue ? Birthday.Value.ToString("MMMM d, yyyy") : "Birthday not set";

    public bool IsBirthdayToday => Birthday.HasValue
        && Birthday.Value.Month == DateTime.Today.Month
        && Birthday.Value.Day == DateTime.Today.Day;

    public bool CanSendFriendRequest => !IsOwnProfile && !IsBlocked && !RelationshipStatus.HasValue;

    public bool HasMutualFriends => MutualFriends.Count > 0;

    public string? ActionMessage { get; set; }

    public bool HasMatchmakingProfile { get; set; }

    public bool IsMatchmakingEnabled { get; set; }

    public static ProfileViewModel FromUser(
        User user,
        bool isOwnProfile,
        bool isBlocked,
        FriendStatus? relationshipStatus,
        List<FriendListItemViewModel> mutualFriends,
        string? actionMessage)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new ProfileViewModel
        {
            UserId = user.Id,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
            AvatarInitial = string.IsNullOrWhiteSpace(user.Username)
                ? "?"
                : user.Username[..1].ToUpperInvariant(),
            Bio = user.Bio,
            Status = user.Status,
            Birthday = user.Birthday,
            IsOwnProfile = isOwnProfile,
            IsBlocked = isBlocked,
            RelationshipStatus = relationshipStatus,
            MutualFriends = mutualFriends,
            ActionMessage = actionMessage,
        };
    }
}
