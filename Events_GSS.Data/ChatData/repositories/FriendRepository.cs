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
    public class FriendRepository : IFriendRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public FriendRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Friend?> GetFriendshipAsync(Guid firstUserId, Guid secondUserId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Friends.FirstOrDefaultAsync(friendship =>
                (friendship.UserId1 == firstUserId && friendship.UserId2 == secondUserId) ||
                (friendship.UserId1 == secondUserId && friendship.UserId2 == firstUserId));
        }

        public async Task<List<Friend>> GetAllFriendshipsForUserAsync(Guid targetUserId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Friends
                .Where(friendship => friendship.UserId1 == targetUserId || friendship.UserId2 == targetUserId)
                .ToListAsync();
        }

        public async Task<List<Friend>> GetPendingRequestsForUserAsync(Guid targetUserId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Friends
                .Where(friendship => friendship.UserId2 == targetUserId && friendship.Status == FriendStatus.Pending)
                .ToListAsync();
        }

        public async Task<List<Friend>> GetAcceptedFriendsAsync(Guid targetUserId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Friends
                .Where(friendship =>
                    (friendship.UserId1 == targetUserId || friendship.UserId2 == targetUserId) &&
                    friendship.Status == FriendStatus.Accepted)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetMutualFriendIdentifiersAsync(Guid firstUserId, Guid secondUserId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var firstUserFriends = await db.Friends
                .Where(friendship =>
                    friendship.Status == FriendStatus.Accepted &&
                    (friendship.UserId1 == firstUserId || friendship.UserId2 == firstUserId))
                .Select(friendship => friendship.UserId1 == firstUserId ? friendship.UserId2 : friendship.UserId1)
                .ToListAsync();

            var secondUserFriends = await db.Friends
                .Where(friendship =>
                    friendship.Status == FriendStatus.Accepted &&
                    (friendship.UserId1 == secondUserId || friendship.UserId2 == secondUserId))
                .Select(friendship => friendship.UserId1 == secondUserId ? friendship.UserId2 : friendship.UserId1)
                .ToListAsync();

            return firstUserFriends
                .Intersect(secondUserFriends)
                .ToList();
        }

        public async Task<bool> CheckIfFriendsAsync(Guid firstUserId, Guid secondUserId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Friends.AnyAsync(friendship =>
                friendship.Status == FriendStatus.Accepted &&
                ((friendship.UserId1 == firstUserId && friendship.UserId2 == secondUserId) ||
                 (friendship.UserId1 == secondUserId && friendship.UserId2 == firstUserId)));
        }

        public async Task CreateFriendshipAsync(Friend newFriendship)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            db.Friends.Add(newFriendship);
            await db.SaveChangesAsync();
        }

        public async Task UpdateFriendshipStatusAsync(Guid firstUserId, Guid secondUserId, FriendStatus newStatus)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var friendship = await db.Friends.FirstOrDefaultAsync(friendship =>
                (friendship.UserId1 == firstUserId && friendship.UserId2 == secondUserId) ||
                (friendship.UserId1 == secondUserId && friendship.UserId2 == firstUserId));
            if (friendship == null) return;
            friendship.Status = newStatus;
            await db.SaveChangesAsync();
        }

        public async Task SetMatchStatusAsync(Guid firstUserId, Guid secondUserId, bool isMatchStatus)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var friendship = await db.Friends.FirstOrDefaultAsync(friendship =>
                (friendship.UserId1 == firstUserId && friendship.UserId2 == secondUserId) ||
                (friendship.UserId1 == secondUserId && friendship.UserId2 == firstUserId));
            if (friendship == null) return;
            friendship.IsMatch = isMatchStatus;
            await db.SaveChangesAsync();
        }

        public async Task DeleteFriendshipAsync(Guid firstUserId, Guid secondUserId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            var friendship = await db.Friends.FirstOrDefaultAsync(friendship =>
                (friendship.UserId1 == firstUserId && friendship.UserId2 == secondUserId) ||
                (friendship.UserId1 == secondUserId && friendship.UserId2 == firstUserId));
            if (friendship == null) return;
            db.Friends.Remove(friendship);
            await db.SaveChangesAsync();
        }
    }
}
