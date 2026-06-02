using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class AttendedEventsViewModel
{
    public List<AttendedEvent> AttendedEvents { get; set; } = new();
    public List<AttendedEvent> ArchivedEvents { get; set; } = new();
    public List<AttendedEvent> FavouriteEvents { get; set; } = new();
    public List<AttendedEvent> CommonEvents { get; set; } = new();
    public List<Category> AvailableCategories { get; set; } = new();

    public string SearchQuery { get; set; } = string.Empty;
    public int? SelectedCategoryId { get; set; }
    public string SelectedSort { get; set; } = "Default";

    public string? ErrorMessage { get; set; }
}