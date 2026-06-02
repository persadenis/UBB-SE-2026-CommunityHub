using System.Collections.Generic;
using System;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class DiscussionMessageViewModel
{
    public DiscussionMessage Message { get; set; } = null!;
    public List<ReactionGroup> ReactionGroups { get; set; } = new();
    public string? CurrentUserEmoji { get; set; }
    public bool ShowMuteButton { get; set; }
}

public class DiscussionViewModel
{
    public int EventId { get; set; }
    public bool IsEventAdmin { get; set; }
    public bool IsMuted { get; set; }
    public string? MuteRemainingText { get; set; }
    public int? CurrentSlowModeSeconds { get; set; }
    public bool IsSlowModeActive => CurrentSlowModeSeconds.HasValue && CurrentSlowModeSeconds.Value > 0;
    public string SlowModeStatusText => IsSlowModeActive
        ? $"Slow mode: {CurrentSlowModeSeconds}s between messages"
        : "Slow mode: Off";
    public List<DiscussionMessageViewModel> Messages { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public Guid CurrentUserId { get; set; }
}