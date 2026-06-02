using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories;

public class QuestMemoryRepository : IQuestMemoryRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public QuestMemoryRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> AddMemoryAsync(Memory proofMemory)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        db.Memories.Add(proofMemory);
        await db.SaveChangesAsync();
        return proofMemory.MemoryId;
    }

    public async Task SubmitProofAsync(Quest quest, Memory proof)
    {
        var questMemory = new QuestMemory
        {
            ForQuest = quest,
            Proof = proof,
            QuestId = quest.Id,
            MemoryId = proof.MemoryId,
            ProofStatus = QuestMemoryStatus.Submitted
        };
        using var db = await _contextFactory.CreateDbContextAsync();
        db.Set<QuestMemory>().Add(questMemory);
        await db.SaveChangesAsync();
    }

    public async Task<List<QuestMemory>> GetRawSubmissionsForUser(User user)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<QuestMemory>()
            .Include(qm => qm.ForQuest)
            .Include(qm => qm.Proof)
                .ThenInclude(m => m.Author)
            .Where(qm => qm.Proof.AuthorId == user.UserId)
            .ToListAsync();
    }

    public async Task<List<QuestMemory>> GetProofsForQuestAsync(Quest quest)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<QuestMemory>()
            .Include(qm => qm.Proof)
                .ThenInclude(m => m.Author)
            .Include(qm => qm.ForQuest)
            .Where(qm => qm.QuestId == quest.Id && qm.ProofStatus == QuestMemoryStatus.Submitted)
            .ToListAsync();
    }

    public async Task ChangeProofStatusAsync(QuestMemory proof)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var questMemory = await db.Set<QuestMemory>()
            .FirstOrDefaultAsync(qm => qm.QuestId == proof.QuestId && qm.MemoryId == proof.MemoryId);

        if (questMemory != null)
        {
            questMemory.ProofStatus = proof.ProofStatus;
            await db.SaveChangesAsync();
        }
    }

    public async Task DeleteProofAsync(QuestMemory proof)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var questMemory = await db.Set<QuestMemory>()
            .FirstOrDefaultAsync(qm => qm.QuestId == proof.QuestId && qm.MemoryId == proof.MemoryId);

        if (questMemory != null)
        {
            db.Set<QuestMemory>().Remove(questMemory);
            await db.SaveChangesAsync();
        }
    }
}