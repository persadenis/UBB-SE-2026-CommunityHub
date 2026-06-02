using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.ChatData.repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UserRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Users.ToListAsync();
        }

        public async Task<List<User>> SearchByUsernameAsync(string query)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Users
                .Where(u => u.Username.Contains(query))
                .ToListAsync();
        }

        public async Task CreateAsync(User user)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Users.Add(user);

            var eventsUser = new ChatAndEvents.Data.EventsData.Models.User
            {
                UserId = user.Id,
                Name = user.Username,
                ReputationPoints = 0
            };
            db.Set<ChatAndEvents.Data.EventsData.Models.User>().Add(eventsUser);

            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Users.Update(user);
            await db.SaveChangesAsync();
        }
          
        public async Task UpdatePasswordAsync(Guid id, string passwordHash)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var user = await db.Users.FindAsync(id);
            if (user == null) return;
            user.PasswordHash = passwordHash;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
    }
}