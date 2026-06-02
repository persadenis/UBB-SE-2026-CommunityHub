using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class CreateEventViewModel
{
    private static readonly List<Category> DefaultCategories =
    [
        new Category { CategoryId = 1, Title = "NATURE" },
        new Category { CategoryId = 2, Title = "FITNESS" },
        new Category { CategoryId = 3, Title = "MUSIC" },
        new Category { CategoryId = 4, Title = "SOCIAL" },
        new Category { CategoryId = 5, Title = "ART" },
        new Category { CategoryId = 6, Title = "PETS" },
        new Category { CategoryId = 7, Title = "TECH" },
        new Category { CategoryId = 8, Title = "FUN" },
    ];

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

    public List<Quest> AvailableQuests { get; set; } = new();

    public List<CreateEventSelectedQuestViewModel> SelectedQuests { get; set; } = new();

    public string CustomQuestName { get; set; } = string.Empty;

    public string CustomQuestDescription { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public bool IsStep1Visible => CurrentStep == 1;

    public bool IsStep2Visible => CurrentStep == 2;

    public bool IsStep3Visible => CurrentStep == 3;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasBannerImage => !string.IsNullOrWhiteSpace(EventBannerPath);

    public IReadOnlyList<Category> AvailableCategories => DefaultCategories;

    public Category? SelectedCategory =>
        SelectedCategoryId.HasValue
            ? DefaultCategories.FirstOrDefault(category => category.CategoryId == SelectedCategoryId.Value)
            : null;

    public CreateEventWizardState ToState()
    {
        return new CreateEventWizardState
        {
            CurrentStep = CurrentStep,
            EventName = EventName,
            Location = Location,
            StartDate = StartDate,
            StartTime = StartTime,
            EndDate = EndDate,
            EndTime = EndTime,
            IsPublic = IsPublic,
            Description = Description,
            MaximumPeopleText = MaximumPeopleText,
            EventBannerPath = EventBannerPath,
            SelectedCategoryId = SelectedCategoryId,
            SelectedQuests = SelectedQuests
                .Select(quest => new CreateEventSelectedQuestViewModel
                {
                    Key = quest.Key,
                    PresetQuestId = quest.PresetQuestId,
                    Name = quest.Name,
                    Description = quest.Description,
                    Difficulty = quest.Difficulty
                })
                .ToList()
        };
    }

    public static CreateEventViewModel FromState(CreateEventWizardState state, List<Quest> availableQuests)
    {
        return new CreateEventViewModel
        {
            CurrentStep = state.CurrentStep,
            EventName = state.EventName,
            Location = state.Location,
            StartDate = state.StartDate,
            StartTime = state.StartTime,
            EndDate = state.EndDate,
            EndTime = state.EndTime,
            IsPublic = state.IsPublic,
            Description = state.Description,
            MaximumPeopleText = state.MaximumPeopleText,
            EventBannerPath = state.EventBannerPath,
            SelectedCategoryId = state.SelectedCategoryId,
            AvailableQuests = availableQuests,
            SelectedQuests = state.SelectedQuests
                .Select(quest => new CreateEventSelectedQuestViewModel
                {
                    Key = quest.Key,
                    PresetQuestId = quest.PresetQuestId,
                    Name = quest.Name,
                    Description = quest.Description,
                    Difficulty = quest.Difficulty
                })
                .ToList()
        };
    }
}
