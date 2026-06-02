using System.Collections.Generic;
using System.Linq;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;

namespace ChatAndEvents.Data.EventsData.ViewModelsCore;

public class QuestUserCore
{
    private readonly IQuestApprovalService _questService;

    public QuestUserCore(IQuestApprovalService questService)
    {
        _questService = questService;
    }

    public async Task<List<QuestMemory>> GetQuestsAsync(Event currentEvent, User user)
    {
        return await _questService.GetQuestsWithStatus(currentEvent, user);
    }

    public List<QuestMemory> FilterQuests(List<QuestMemory> allQuests, QuestMemoryStatus? filterStatus)
    {
        if (filterStatus == null) return allQuests;

        return allQuests.Where(q => q.ProofStatus == filterStatus).ToList();
    }
}