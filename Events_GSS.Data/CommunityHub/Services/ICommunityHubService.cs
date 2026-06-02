using ChatAndEvents.Data.CommunityHub.Models;

namespace ChatAndEvents.Data.CommunityHub.Services;

public interface ICommunityHubService
{
    Task<IReadOnlyList<CommunitySearchResult>> SearchAsync(Guid currentUserId, string? query, string? category);

    Task<HubCommunity?> GetCommunityAsync(Guid communityId, Guid currentUserId);

    Task<IReadOnlyList<string>> GetCategoriesAsync();

    Task<HubCommunity> CreateAsync(Guid ownerId, string name, string description, string category);

    Task JoinAsync(Guid communityId, Guid userId);

    Task LeaveAsync(Guid communityId, Guid userId);

    Task AddPostAsync(Guid communityId, Guid authorId, string title, string body);
}
