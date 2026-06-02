using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class EventListingViewModel
{
    public List<Event> Events { get; set; } = new();

    public string? SearchQuery { get; set; }

    public string? LocationFilter { get; set; }

    public string? ErrorMessage { get; set; }
}
