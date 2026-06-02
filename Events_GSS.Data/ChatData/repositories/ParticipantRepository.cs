using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.ChatData.repositories
{
    public class ParticipantRepository : IParticipantRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ParticipantRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Participant?> GetAsync(Guid conversationId, Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
        }

        public async Task<List<Participant>> GetAllForConversationAsync(Guid conversationId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Participants
                .Where(p => p.ConversationId == conversationId)
                .ToListAsync();
        }

        public async Task<List<Participant>> GetAllForUserAsync(Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Participants
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task CreateAsync(Participant participant)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Participants.Add(participant);
            await db.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(Guid conversationId, Guid userId, ParticipantRole role)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var participant = await db.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
            if (participant == null) return;
            participant.Role = role;
            await db.SaveChangesAsync();
        }

        public async Task UpdateLastReadAsync(Guid conversationId, Guid userId, Guid messageId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var participant = await db.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
            if (participant == null) return;
            participant.LastReadMessageId = messageId;
            await db.SaveChangesAsync();
        }

        public async Task UpdateTimeoutAsync(Guid conversationId, Guid userId, DateTime? until)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var participant = await db.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
            if (participant == null) return;
            participant.TimeoutUntil = until;
            await db.SaveChangesAsync();
        }

        public async Task UpdateFavouriteAsync(Guid conversationId, Guid userId, bool isFav)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var participant = await db.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
            if (participant == null) return;
            participant.IsFavourite = isFav;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid conversationId, Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var participant = await db.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
            if (participant == null) return;
            db.Participants.Remove(participant);
            await db.SaveChangesAsync();
        }

        public async Task UpdateNicknameAsync(Guid conversationId, Guid userId, string? nickname)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var participant = await db.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
            if (participant == null) return;
            participant.Nickname = nickname;
            await db.SaveChangesAsync();
        }
    }
}
