using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories;

public class QuestRepository : IQuestRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public QuestRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> AddQuestAsync(Event toEvent, Quest quest)
    {
        quest.EventId = toEvent.EventId;
        using var db = await _contextFactory.CreateDbContextAsync();
        db.Set<Quest>().Add(quest);
        await db.SaveChangesAsync();
        return quest.Id;
    }

    public async Task<List<Quest>> GetQuestsAsync(Event fromEvent)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<Quest>()
            .Include(q => q.PrerequisiteQuest)
            .Where(q => q.EventId == fromEvent.EventId)
            .ToListAsync();
    }

    public async Task<Quest> GetQuestByIdAsync(int questId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<Quest>()
            .Include(q => q.PrerequisiteQuest)
            .FirstOrDefaultAsync(q => q.Id == questId);
    }

    public async Task DeleteQuestAsync(Quest quest)
    {
        // Delete QuestMemories first due to NoAction on delete
        using var db = await _contextFactory.CreateDbContextAsync();
        await db.Set<QuestMemory>()
            .Where(qm => qm.QuestId == quest.Id)
            .ExecuteDeleteAsync();

        var questEntity = await db.Set<Quest>().FindAsync(quest.Id);
        if (questEntity != null)
        {
            db.Set<Quest>().Remove(questEntity);
            await db.SaveChangesAsync();
        }
    }
}