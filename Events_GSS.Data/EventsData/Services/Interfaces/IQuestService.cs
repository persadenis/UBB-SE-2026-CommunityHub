using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.Interfaces;

public interface IQuestService
{
    /// <summary>
    /// Adds a new quest. Does not matter if quest is preset or custom.
    /// </summary>
    Task<int> AddQuestAsync(Event toEvent , Quest quest);

    /// <summary>
    /// Gets all quests for a specific event.
    /// </summary>
    Task<List<Quest>> GetQuestsAsync(Event fromEvent);

    /// <summary>
    /// Deletes a quest.
    /// </summary>
    Task DeleteQuestAsync(Quest quest);

    Task<List<Quest>> GetPresetQuestsAsync();
}