namespace ChatAndEvents.Web.Models;

public class QuestUserViewModel
{
    public int EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public string Filter { get; set; } = QuestUserFilters.All;

    public List<QuestItemViewModel> Quests { get; set; } = new();

    public string StatusText { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }
}

public static class QuestUserFilters
{
    public const string All = "All";
    public const string Submitted = "Submitted";
    public const string Completed = "Completed";
    public const string Incomplete = "Incomplete";
}
