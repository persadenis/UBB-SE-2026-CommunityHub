using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.ChatData.repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ConversationRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Conversation?> GetByIdAsync(Guid id)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Conversations.FirstOrDefaultAsync(conversation => conversation.Id == id);
        }

        public async Task<List<Conversation>> GetAllForUserAsync(Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await (from conversation in db.Conversations
                          join participant in db.Participants on conversation.Id equals participant.ConversationId
                          where participant.UserId == userId
                          select conversation)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Conversation?> GetDmBetweenAsync(Guid userId1, Guid userId2)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Conversations
                .Where(conversation => conversation.Type == ConversationType.Dm)
                .Where(conversation =>
                    db.Participants
                        .Where(participant =>
                            participant.ConversationId == conversation.Id &&
                            (participant.UserId == userId1 || participant.UserId == userId2))
                        .Select(participant => participant.UserId)
                        .Distinct()
                        .Count() == 2)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Conversation c)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Conversations.Add(c);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Conversation c)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var conversation = await db.Conversations.FindAsync(c.Id);
            if (conversation == null)
            {
                return;
            }

            conversation.Title = c.Title;
            conversation.IconUrl = c.IconUrl;
            await db.SaveChangesAsync();
        }

        public async Task SetPinnedMessageAsync(Guid conversationId, Guid? messageId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var conversation = await db.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return;
            }

            conversation.PinnedMessageId = messageId;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid conversationId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var messages = await db.Messages
                .Where(message => message.ConversationId == conversationId)
                .ToListAsync();

            var participants = await db.Participants
                .Where(participant => participant.ConversationId == conversationId)
                .ToListAsync();

            var conversation = await db.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return;
            }

            db.Messages.RemoveRange(messages);
            db.Participants.RemoveRange(participants);
            db.Conversations.Remove(conversation);
            await db.SaveChangesAsync();
        }
    }
}
