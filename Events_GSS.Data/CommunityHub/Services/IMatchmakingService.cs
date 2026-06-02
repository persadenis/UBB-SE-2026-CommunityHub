using ChatAndEvents.Data.CommunityHub.Models;

namespace ChatAndEvents.Data.CommunityHub.Services;

public interface IMatchmakingService
{
    Task<DatingProfile?> GetProfileAsync(Guid userId);

    Task<DatingProfile> SaveProfileAsync(
        Guid userId,
        string displayName,
        string gender,
        string preferredGenders,
        string location,
        string datingBio,
        string interests,
        string photoUrls,
        int minPreferredAge,
        int maxPreferredAge,
        int maxDistanceKm,
        string loverType,
        bool isEnabled);

    Task DisableAsync(Guid userId);

    Task<IReadOnlyList<DatingCandidate>> GetCandidatesAsync(Guid userId);

    Task<IReadOnlyList<DatingCandidate>> GetMatchesAsync(Guid userId);

    Task<bool> ReactAsync(Guid userId, Guid targetUserId, string action);
}
