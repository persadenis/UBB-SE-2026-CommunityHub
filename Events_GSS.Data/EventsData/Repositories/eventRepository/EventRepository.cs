namespace ChatAndEvents.Data.EventsData.Repositories.eventRepository;

using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

public class EventRepository : IEventRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public EventRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Event>> GetAllPublicActiveAsync()
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Events
            .Include(e => e.Category)
            .Include(e => e.Admin)
            .Where(e => e.IsPublic && e.EndDateTime > DateTime.UtcNow)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int eventId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Events
            .Include(e => e.Category)
            .Include(e => e.Admin)
            .FirstOrDefaultAsync(e => e.EventId == eventId);
    }

    public async Task<int> AddAsync(Event eventEntity)
    {
        if (eventEntity.Admin == null)
            throw new ArgumentException("Admin is required.", nameof(eventEntity));

        eventEntity.EnrolledCount = 0;

        using var db = await _contextFactory.CreateDbContextAsync();
        if (eventEntity.Category != null)
        {
            eventEntity.CategoryId = eventEntity.Category.CategoryId;
            db.Entry(eventEntity.Category).State = EntityState.Unchanged;
        }

        if (eventEntity.Admin != null)
        {
            eventEntity.AdminId = eventEntity.Admin.UserId;
            db.Entry(eventEntity.Admin).State = EntityState.Unchanged;
        }

        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();

        // Auto-enroll the admin as a participant
        var attendedEvent = new AttendedEvent
        {
            EventId = eventEntity.EventId,
            UserId = (Guid)eventEntity.AdminId
        };
        db.AttendedEvents.Add(attendedEvent);
        await db.SaveChangesAsync();

        return eventEntity.EventId;
    }

    public async Task IncrementEnrolledCountAsync(int eventId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var eventEntity = await db.Events.FindAsync(eventId);
        if (eventEntity != null)
        {
            eventEntity.EnrolledCount++;
            await db.SaveChangesAsync();
        }
    }

    public async Task DecrementEnrolledCountAsync(int eventId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var eventEntity = await db.Events.FindAsync(eventId);
        if (eventEntity != null && eventEntity.EnrolledCount > 0)
        {
            eventEntity.EnrolledCount--;
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(Event eventEntity)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        db.Events.Update(eventEntity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int eventId)
    {
        // Handle children manually since we use NoAction on delete
        using var db = await _contextFactory.CreateDbContextAsync();
        var announcementIds = await db.Set<Announcement>()
            .Where(a => a.EventId == eventId)
            .Select(a => a.AnnouncementId)
            .ToListAsync();

        await db.Set<AnnouncementReadReceipt>()
            .Where(r => announcementIds.Contains(r.AnnouncementId))
            .ExecuteDeleteAsync();

        await db.Set<AnnouncementReaction>()
            .Where(r => announcementIds.Contains(r.AnnouncementId))
            .ExecuteDeleteAsync();

        await db.Set<Announcement>()
            .Where(a => a.EventId == eventId)
            .ExecuteDeleteAsync();

        var memoryIds = await db.Memories
            .Where(m => m.EventId == eventId)
            .Select(m => m.MemoryId)
            .ToListAsync();

        await db.Set<MemoryLike>()
            .Where(ml => memoryIds.Contains(ml.MemoryId))
            .ExecuteDeleteAsync();

        await db.Set<QuestMemory>()
            .Where(qm => memoryIds.Contains(qm.MemoryId))
            .ExecuteDeleteAsync();

        await db.Memories
            .Where(m => m.EventId == eventId)
            .ExecuteDeleteAsync();

        await db.AttendedEvents
            .Where(ae => ae.EventId == eventId)
            .ExecuteDeleteAsync();

        await db.Set<Quest>()
            .Where(q => q.EventId == eventId)
            .ExecuteDeleteAsync();

        var eventEntity = await db.Events.FindAsync(eventId);
        if (eventEntity != null)
        {
            db.Events.Remove(eventEntity);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<Event>> GetByAdminIdAsync(Guid adminId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Events
            .Include(e => e.Category)
            .Include(e => e.Admin)
            .Where(e => e.AdminId == adminId)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }
}