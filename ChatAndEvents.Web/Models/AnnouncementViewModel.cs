using System.Collections.Generic;

namespace ChatAndEvents.Web.Models;

public class AnnouncementViewModel
{
    public int EventId { get; set; }

    public string CurrentUserName { get; set; } = string.Empty;

    public bool IsEventAdmin { get; set; }

    public List<AnnouncementItemViewModel> Announcements { get; set; } = new();

    public string NewMessage { get; set; } = string.Empty;

    public AnnouncementItemViewModel? EditingAnnouncement { get; set; }

    public bool IsLoading { get; set; }

    public string? ErrorMessage { get; set; }

    public int UnreadCount { get; set; }

    public bool HasAnnouncements => Announcements.Count > 0;

    public bool IsEditing => EditingAnnouncement is not null;

    public string CreateButtonText => IsEditing ? "Save Edit" : "Post";
}
