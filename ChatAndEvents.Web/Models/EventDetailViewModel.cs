using System;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class EventDetailViewModel
{
    public Event SelectedEvent { get; set; } = new();

    public Guid CurrentUserId { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsEnrolled { get; set; }

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public bool CanShowStatistics => IsAdmin;

    public string JoinLeaveButtonText => IsEnrolled ? "Leave Event" : "Join Event";
}
