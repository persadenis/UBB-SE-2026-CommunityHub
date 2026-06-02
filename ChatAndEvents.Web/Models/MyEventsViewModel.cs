using ChatAndEvents.Data.EventsData.Models;
using System.Collections.Generic;

namespace ChatAndEvents.Web.Models;

public class MyEventsViewModel
{
    public List<Event> Events { get; set; } = new();
    public bool IsEmpty => Events.Count == 0;
    public string? ErrorMessage { get; set; }
}