using ChatAndEvents.Data.EventsData.Models;
using System.Collections.Generic;

namespace ChatAndEvents.Web.Models;

public class NotificationViewModel
{
    public List<Notification> Notifications { get; set; } = new();
    public string? ErrorMessage { get; set; }
}