using System;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Web.Models;

public class FriendListItemViewModel
{
    public FriendListItemViewModel(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Id = user.Id;
        Username = user.Username;
        AvatarUrl = user.AvatarUrl;
        AvatarInitial = string.IsNullOrWhiteSpace(user.Username)
            ? "?"
            : user.Username[..1].ToUpperInvariant();
        HasAvatar = !string.IsNullOrWhiteSpace(user.AvatarUrl);
        Status = user.Status;
        StatusLabel = user.Status.ToString();
        Bio = user.Bio;
        IsBirthdayToday = user.Birthday.HasValue
            && user.Birthday.Value.Month == DateTime.Today.Month
            && user.Birthday.Value.Day == DateTime.Today.Day;
    }

    public Guid Id { get; }

    public string Username { get; }

    public string? AvatarUrl { get; }

    public string AvatarInitial { get; }

    public bool HasAvatar { get; }

    public UserStatus Status { get; }

    public string StatusLabel { get; }

    public string? Bio { get; }

    public bool IsBirthdayToday { get; }
}
