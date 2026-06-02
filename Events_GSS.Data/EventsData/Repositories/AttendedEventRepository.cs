using System.Data;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories
{
    public class AttendedEventRepository : IAttendedEventRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public AttendedEventRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        public async Task AddAsync(AttendedEvent attendedEventEntity)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.AttendedEvents.Add(attendedEventEntity);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int eventId, Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var attendedEvent = await db.AttendedEvents.FindAsync(eventId, userId);
            if (attendedEvent != null)
            {
                db.AttendedEvents.Remove(attendedEvent);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateIsArchivedAsync(int eventId, Guid userId, bool isArchived)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var attendedEvent = await db.AttendedEvents.FindAsync(eventId, userId);
            if (attendedEvent != null)
            {
                attendedEvent.IsArchived = isArchived;
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateIsFavouriteAsync(int eventId, Guid userId, bool isFavourite)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var attendedEvent = await db.AttendedEvents.FindAsync(eventId, userId);
            if (attendedEvent != null)
            {
                attendedEvent.IsFavourite = isFavourite;
                await db.SaveChangesAsync();
            }
        }

        public async Task<AttendedEvent?> GetAsync(int eventId, Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == userId);
        }

        public async Task<List<AttendedEvent>> GetByUserIdAsync(Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<AttendedEvent>> GetCommonEventsAsync(Guid userId, Guid friendId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var userEvents = await db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var friendEvents = await db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.UserId == friendId)
                .ToListAsync();

            return userEvents.Where(ue => friendEvents.Any(fe => fe.EventId == ue.EventId)).ToList();
        }

        public async Task<int> GetAttendeeCountAsync(int eventId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.EventId == eventId)
                .CountAsync();
        }
    }
}