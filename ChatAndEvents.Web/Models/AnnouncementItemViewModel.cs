using System;
using System.Collections.Generic;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.ViewModelsCore;

namespace ChatAndEvents.Web.Models;

public class AnnouncementItemViewModel
{
    private readonly AnnouncementItemViewModelCore _core;

    public AnnouncementItemViewModel(Announcement announcementModel, Guid currentUserId, bool isAdmin)
    {
        _core = new AnnouncementItemViewModelCore(announcementModel, currentUserId);
        Model = announcementModel;
        IsCurrentUserAdmin = isAdmin;
        IsRead = announcementModel.IsRead;
    }

    public Announcement Model { get; }

    public string PreviewText => _core.PreviewText;

    public bool HasFullContent => _core.HasFullContent;

    public List<ReactionGroup> ReactionGroups => _core.ReactionGroups;

    public string? CurrentUserEmoji => _core.CurrentUserEmoji;

    public bool IsCurrentUserAdmin { get; }

    public bool IsRead { get; }

    public bool IsUnread => !IsRead;

    public string AuthorName => Model.Author?.Name ?? "Unknown";

    public string DateText => Model.Date.ToString("MMM dd, yyyy HH:mm");
}
