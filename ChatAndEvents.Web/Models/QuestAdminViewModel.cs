using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class QuestAdminViewModel
{
    public int EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public List<Quest> Quests { get; set; } = new();

    public List<Quest> PresetQuests { get; set; } = new();

    public string NewQuestName { get; set; } = string.Empty;

    public string NewQuestDescription { get; set; } = string.Empty;

    public int NewQuestDifficulty { get; set; } = 1;

    public int? SelectedPresetQuestId { get; set; }

    public int? SelectedQuestId { get; set; }

    public int? SelectedPrerequisiteQuestId { get; set; }

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public Quest? SelectedQuest => Quests.FirstOrDefault(quest => quest.Id == SelectedQuestId);
}
