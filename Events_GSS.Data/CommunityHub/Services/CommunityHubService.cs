using ChatAndEvents.Data.CommunityHub.Models;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.CommunityHub.Services;

public class CommunityHubService : ICommunityHubService
{
    private static readonly string[] DefaultCategories =
    [
        "Technology",
        "Sports",
        "Music",
        "Art",
        "Gaming",
        "Study",
        "Fitness",
        "General",
    ];

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CommunityHubService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<CommunitySearchResult>> SearchAsync(Guid currentUserId, string? query, string? category)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var communitiesQuery = db.HubCommunities
            .Include(community => community.Members)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            communitiesQuery = communitiesQuery.Where(community =>
                community.Name.Contains(query) || community.Description.Contains(query));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            communitiesQuery = communitiesQuery.Where(community => community.Category == category);
        }

        var communities = await communitiesQuery
            .OrderBy(community => community.Category)
            .ThenBy(community => community.Name)
            .ToListAsync();

        return communities
            .Select(community => new CommunitySearchResult(
                community,
                community.Members.Count,
                community.Members.Any(member => member.UserId == currentUserId)))
            .ToList();
    }

    public async Task<HubCommunity?> GetCommunityAsync(Guid communityId, Guid currentUserId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.HubCommunities
            .Include(community => community.Members)
            .ThenInclude(member => member.User)
            .Include(community => community.Posts.OrderByDescending(post => post.CreatedAt))
            .ThenInclude(post => post.Author)
            .FirstOrDefaultAsync(community => community.Id == communityId);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync()
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var storedCategories = await db.HubCommunities
            .Select(community => community.Category)
            .Distinct()
            .ToListAsync();

        return DefaultCategories
            .Concat(storedCategories)
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(category => category)
            .ToList();
    }

    public async Task<HubCommunity> CreateAsync(Guid ownerId, string name, string description, string category, string? bannerUrl = null)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var community = new HubCommunity
        {
            OwnerId = ownerId,
            Name = name.Trim(),
            Description = description.Trim(),
            Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
            BannerUrl = string.IsNullOrWhiteSpace(bannerUrl) ? null : bannerUrl,
        };

        community.Members.Add(new CommunityMembership
        {
            CommunityId = community.Id,
            UserId = ownerId,
            IsAdmin = true,
        });

        db.HubCommunities.Add(community);
        await db.SaveChangesAsync();
        return community;
    }

    public async Task JoinAsync(Guid communityId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var exists = await db.CommunityMemberships
            .AnyAsync(member => member.CommunityId == communityId && member.UserId == userId);

        if (exists)
        {
            return;
        }

        db.CommunityMemberships.Add(new CommunityMembership
        {
            CommunityId = communityId,
            UserId = userId,
        });

        await db.SaveChangesAsync();
    }

    public async Task LeaveAsync(Guid communityId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var membership = await db.CommunityMemberships
            .FindAsync(communityId, userId);

        if (membership == null || membership.IsAdmin)
        {
            return;
        }

        db.CommunityMemberships.Remove(membership);
        await db.SaveChangesAsync();
    }

    public async Task AddPostAsync(Guid communityId, Guid authorId, string title, string body)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var isMember = await db.CommunityMemberships
            .AnyAsync(member => member.CommunityId == communityId && member.UserId == authorId);

        if (!isMember)
        {
            throw new InvalidOperationException("Join the community before posting.");
        }

        db.CommunityPosts.Add(new CommunityPost
        {
            CommunityId = communityId,
            AuthorId = authorId,
            Title = title.Trim(),
            Body = body.Trim(),
        });

        await db.SaveChangesAsync();
    }
}
