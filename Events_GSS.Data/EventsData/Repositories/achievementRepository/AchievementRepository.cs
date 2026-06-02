namespace ChatAndEvents.Data.EventsData.Repositories.achievementRepository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

public class AchievementRepository : IAchievementRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AchievementRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public async Task<int> GetAttendedEventsCountAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.AttendedEvents
            .AsNoTracking()
            .CountAsync(ae => ae.UserId == userId);
    }
    
    public async Task<int> GetCreatedEventsCountAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Events
            .AsNoTracking()
            .CountAsync(e => e.AdminId == userId);
    }
    
    public async Task<int> GetApprovedQuestsCountAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.QuestMemories
            .AsNoTracking()
            .Where(qm => qm.Proof.AuthorId == userId && qm.ProofStatus == QuestMemoryStatus.Approved)
            .CountAsync();
    }
    
    public async Task<int> GetMemoriesWithPhotosCountAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Memories
            .AsNoTracking()
            .CountAsync(m => m.AuthorId == userId && m.PhotoPath != null);
    }
    
    public async Task<int> GetMessagesCountAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.DiscussionMessages
            .AsNoTracking()
            .CountAsync(dm => dm.Author!.UserId == userId);
    }
    
    public async Task<bool> HasPerfectEventAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();

        var attendedEventIds = await db.AttendedEvents
            .AsNoTracking()
            .Where(ae => ae.UserId == userId)
            .Select(ae => ae.EventId)
            .ToListAsync();
        
        foreach (var eventId in attendedEventIds)
        {
            var quests = await db.Quests
                .AsNoTracking()
                .Where(q => q.EventId == eventId)
                .ToListAsync();
            
            if (!quests.Any())
                continue;
            
            var allQuestsApproved = true;
            foreach (var quest in quests)
            {
                var hasApprovedMemory = await db.QuestMemories
                    .AsNoTracking()
                    .AnyAsync(qm => qm.QuestId == quest.Id && 
                                    qm.Proof.AuthorId == userId && 
                                    qm.ProofStatus == QuestMemoryStatus.Approved);

                if (!hasApprovedMemory)
                {
                    allQuestsApproved = false;
                    break;
                }
            }

            if (allQuestsApproved)
                return true;
        }

        return false;
    }
    
    public async Task<List<Achievement>> GetAllAchievementsAsync()
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Achievements
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<bool> IsAlreadyUnlockedAsync(Guid userId, int achievementId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var achievement = await db.Achievements
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AchievementId == achievementId && a.IsUnlocked);

        return achievement != null;
    }
    
    public async Task UnlockAchievementAsync(Guid userId, int achievementId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var achievement = await db.Achievements.FindAsync(achievementId);
        if (achievement != null)
        {
            achievement.IsUnlocked = true;
            db.Achievements.Update(achievement);
            await db.SaveChangesAsync();
        }
    }
}
