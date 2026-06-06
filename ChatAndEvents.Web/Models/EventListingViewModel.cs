using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class EventListingViewModel
{
    public static readonly IReadOnlyList<string> LocationOptions =
    [
        "Alba Iulia",
        "Arad",
        "Bacau",
        "Baia Mare",
        "Bistrita",
        "Brasov",
        "Bucuresti",
        "Cluj-Napoca",
        "Constanta",
        "Craiova",
        "Deva",
        "Galati",
        "Iasi",
        "Oradea",
        "Pitesti",
        "Ploiesti",
        "Sibiu",
        "Suceava",
        "Targu Mures",
        "Timisoara",
    ];

    public List<Event> Events { get; set; } = new();

    public string? SearchQuery { get; set; }

    public string? LocationFilter { get; set; }

    public string? ErrorMessage { get; set; }
}
