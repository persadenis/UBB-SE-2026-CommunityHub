using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories;
using ChatAndEvents.Data.EventsData.Services.Interfaces;

namespace ChatAndEvents.Data.EventsData.Services;

public class QuestService : IQuestService
{
    private readonly IQuestRepository _questRepository;

    public QuestService(IQuestRepository questRepository)
    {
        _questRepository = questRepository;
    }

    public async Task<int> AddQuestAsync(Event targetEvent, Quest quest)
    {
        return await _questRepository.AddQuestAsync(targetEvent, quest);
    }

    public async Task<List<Quest>> GetQuestsAsync(Event sourceEvent)
    {
        return await _questRepository.GetQuestsAsync(sourceEvent);
    }

    public async Task DeleteQuestAsync(Quest quest)
    {
        await _questRepository.DeleteQuestAsync(quest);
    }

    public async Task<List<Quest>> GetPresetQuestsAsync()
    {
        // Refactoring: Preset quests should be retrieved from the database 
        // or a configuration file, not hardcoded in the service logic.
        return await Task.FromResult(new List<Quest>());
    }
}