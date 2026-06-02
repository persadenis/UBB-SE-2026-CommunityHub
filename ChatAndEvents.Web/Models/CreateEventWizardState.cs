namespace ChatAndEvents.Web.Models;

public class CreateEventWizardState
{
    public int CurrentStep { get; set; } = 1;

    public string EventName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime StartDate { get; set; } = DateTime.Today;

    public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay;

    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    public TimeSpan EndTime { get; set; } = DateTime.Now.AddHours(1).TimeOfDay;

    public bool IsPublic { get; set; } = true;

    public string Description { get; set; } = string.Empty;

    public string MaximumPeopleText { get; set; } = string.Empty;

    public string? EventBannerPath { get; set; }

    public int? SelectedCategoryId { get; set; }

    public List<CreateEventSelectedQuestViewModel> SelectedQuests { get; set; } = new();
}
