// <copyright file="AnnouncementRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories.announcementRepository;

using System.Data;
using ChatAndEvents.Data.EventsData.Models;

using Microsoft.Data.SqlClient;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AnnouncementRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<int> AddAnnouncementAsync(Announcement announcement, int eventId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        if (announcement == null)
        {
            throw new ArgumentNullException(nameof(announcement));
        }

        if (string.IsNullOrWhiteSpace(announcement.Message))
        {
            throw new ArgumentException("Message is required.", nameof(announcement));
        }

        announcement.EventId = eventId;
        announcement.AuthorId = userId;

        db.Announcements.Add(announcement);
        await db.SaveChangesAsync();
        return announcement.AnnouncementId;
    }

    public async Task DeleteAnnouncementAsync(int selectedEvent)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var announcement = db.Announcements.Find(selectedEvent);
        if (announcement != null)
        {
            db.Announcements.Remove(announcement);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetAllParticipantsAsync(int eventId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.AttendedEvents
            .Where(ae => ae.EventId == eventId)
            .Select(ae => ae.User)
            .ToListAsync();
    }

    public async Task<Announcement?> GetAnnouncementByIdAsync(int announcementId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Announcements
            .Include(a => a.Event)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);
    }

    public async Task<List<Announcement>> GetAnnouncementsByEventAsync(int eventId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Announcements
            .Include(a => a.Event)
            .Include(a => a.Author)
            .Where(a => a.EventId == eventId)
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<List<(int AnnouncementId, AnnouncementReaction Reaction)>> GetReactionsAsync(List<int> announcementIds)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var reactions = await db.AnnouncementReactions
            .Include(ar => ar.Author)
            .Where(ar => announcementIds.Contains(ar.AnnouncementId))
            .ToListAsync(); 
        return reactions
            .Select(r=>(r.AnnouncementId, r))
            .ToList();
    }

    public async Task<List<AnnouncementReadReceipt>> GetReadReceiptsAsync(int announcementId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.AnnouncementReadReceipts
            .Where(arr => arr.AnnouncementId == announcementId)
            .ToListAsync();
    }

    public async Task<int> GetTotalParticipantsAsync(int eventId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.AttendedEvents
            .Where(ae => ae.EventId == eventId)
            .CountAsync();
    }

    public async Task<Dictionary<int, int>> GetUnreadCountsForUserAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Announcements
            .Where(a => db.AttendedEvents.Any(ae => ae.UserId == userId && ae.EventId == a.EventId))
            .Where(a => !db.AnnouncementReadReceipts.Any(r => r.AnnouncementId == a.AnnouncementId && r.UserId == userId))
            .GroupBy(a => a.EventId)
            .Select(g => new { EventId = g.Key, UnreadCount = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.UnreadCount);
    }

    public async Task<string?> GetUserReactionAsync(int announcementId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.AnnouncementReactions
            .Where(ar => ar.AnnouncementId == announcementId && ar.AuthorId == userId)
            .Select(ar => ar.Emoji)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasUserReadAsync(int announcementId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.AnnouncementReadReceipts
            .AnyAsync(r => r.AnnouncementId == announcementId && r.UserId == userId);
    }

    public async Task InsertReactionAsync(int announcementId, Guid userId, string emoji)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var announcement = await db.Announcements.FindAsync(announcementId);
        if (announcement == null)
        {
            throw new InvalidOperationException("Announcement not found");
        }

        var user = await db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var announcementReaction = new AnnouncementReaction
        {
            AnnouncementId = announcementId,
            AuthorId = userId,
            Author = user,
            Emoji = emoji
        };

        db.AnnouncementReactions.Add(announcementReaction);
        await db.SaveChangesAsync();
    }

    public async Task InsertReadReceiptAsync(int announcementId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var announcement = await db.Announcements.FindAsync(announcementId);
        if (announcement == null)
        {
            throw new InvalidOperationException("Announcement not found");
        }

        var user = await db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var readReceipt = new AnnouncementReadReceipt
        {
            AnnouncementId = announcementId,
            UserId = userId,
            User = user,
            ReadAt = DateTime.UtcNow,
        };

        db.AnnouncementReadReceipts.Add(readReceipt);
        await db.SaveChangesAsync();
    }

    public async Task PinAsync(int announcementId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var announcement = await db.Announcements.FindAsync(announcementId);
        if (announcement == null)
        {
            throw new InvalidOperationException("Announcement not found");
        }
        announcement.IsPinned = true;
        await db.SaveChangesAsync();
    }

    public async Task RemoveReactionAsync(int announcementId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var announcementReaction=await db.AnnouncementReactions
            .Where(ar => ar.AnnouncementId == announcementId && ar.AuthorId == userId)
            .FirstOrDefaultAsync();
        if (announcementReaction != null)
        {
            db.AnnouncementReactions.Remove(announcementReaction);
            await db.SaveChangesAsync();
        }
    }

    public async Task UnpinAnnouncementAsync(int eventId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        await db.Announcements
            .Where(a => a.EventId == eventId && a.IsPinned)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.IsPinned, false));
    }

    public async Task UpdateAnnouncementAsync(int announcementId, string newMessage)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var announcement = await db.Announcements.FindAsync(announcementId);
        if (announcement == null)
        {
            throw new InvalidOperationException("Announcement not found");
        }
        
        announcement.Message = newMessage;
        await db.SaveChangesAsync();
    }

    public async Task UpdateReactionAsync(int announcementId, Guid userId, string emoji)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var announcementReaction = await db.AnnouncementReactions
            .Where(ar => ar.AnnouncementId == announcementId && ar.AuthorId == userId)
            .FirstOrDefaultAsync();
        if (announcementReaction != null)
        {
            announcementReaction.Emoji = emoji;
            await db.SaveChangesAsync();
        }
    }
}
