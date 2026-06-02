// <copyright file="MemoryRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ChatAndEvents.Data.Database;
    using ChatAndEvents.Data.EventsData.Models;
    using Microsoft.EntityFrameworkCore;

    public class MemoryRepository : IMemoryRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public MemoryRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        public async Task<List<Memory>> GetByEventAsync(int eventId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Memories
                .AsNoTracking()
                .Include(memory => memory.Event)
                    .ThenInclude(@event => @event.Admin)
                .Include(memory => memory.Author)
                .Where(memory => memory.EventId == eventId)
                .OrderByDescending(memory => memory.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> AddAsync(Memory memory)
        {
            if (memory.EventId <= 0 || memory.AuthorId == Guid.Empty)
            {
                throw new ArgumentException("EventId and AuthorId are required.", nameof(memory));
            }

            var memoryEntity = new Memory
            {
                PhotoPath = memory.PhotoPath,
                Text = memory.Text,
                CreatedAt = memory.CreatedAt,
                EventId = memory.EventId,
                AuthorId = memory.AuthorId,
            };
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Memories.Add(memoryEntity);
            await db.SaveChangesAsync();
            memory.MemoryId = memoryEntity.MemoryId;
            return memoryEntity.MemoryId;
        }

        public async Task DeleteAsync(int memoryId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var memory = await db.Memories.FindAsync(memoryId);
            if (memory == null)
            {
                return;
            }

            db.Memories.Remove(memory);
            await db.SaveChangesAsync();
        }

        public async Task AddLikeAsync(int memoryId, Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            if (await db.Memories.FindAsync(memoryId) == null)
            {
                throw new InvalidOperationException("Memory not found.");
            }

            if (await db.Users.FindAsync(userId) == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var memoryLike = new MemoryLike
            {
                MemoryId = memoryId,
                UserId = userId,
            };

            db.MemoryLikes.Add(memoryLike);
            await db.SaveChangesAsync();
        }

        public async Task RemoveLikeAsync(int memoryId, Guid userId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var memoryLike = await db.MemoryLikes.FindAsync(memoryId, userId);
            if (memoryLike == null)
            {
                return;
            }

            db.MemoryLikes.Remove(memoryLike);
            await db.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetLikesAsync(int memoryId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.MemoryLikes
                .AsNoTracking()
                .Where(memoryLike => memoryLike.MemoryId == memoryId)
                .Select(memoryLike => memoryLike.UserId)
                .ToListAsync();
        }

        public async Task<Memory?> GetByIdAsync(int memoryId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Memories
                .AsNoTracking()
                .Include(memory => memory.Event)
                    .ThenInclude(@event => @event.Admin)
                .Include(memory => memory.Author)
                .FirstOrDefaultAsync(memory => memory.MemoryId == memoryId);
        }
    }
}
