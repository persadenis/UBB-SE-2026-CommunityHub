using ChatAndEvents.Data.CommunityHub.Models;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.CommunityHub.Services;

public class MatchmakingService : IMatchmakingService
{
    private static readonly HashSet<string> PositiveActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Like",
        "SuperLike",
    };

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MatchmakingService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<DatingProfile?> GetProfileAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.DatingProfiles
            .Include(profile => profile.Interests.OrderBy(interest => interest.Name))
            .Include(profile => profile.Photos.OrderBy(photo => photo.SortOrder))
            .FirstOrDefaultAsync(profile => profile.UserId == userId);
    }

    public async Task<DatingProfile> SaveProfileAsync(
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
        bool isEnabled)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var profile = await db.DatingProfiles
            .Include(existing => existing.Interests)
            .Include(existing => existing.Photos)
            .FirstOrDefaultAsync(existing => existing.UserId == userId);

        if (profile == null)
        {
            profile = new DatingProfile { UserId = userId };
            db.DatingProfiles.Add(profile);
        }

        profile.DisplayName = displayName.Trim();
        profile.Gender = gender.Trim();
        profile.PreferredGenders = preferredGenders.Trim();
        profile.Location = location.Trim();
        profile.DatingBio = datingBio.Trim();
        profile.MinPreferredAge = Math.Max(18, minPreferredAge);
        profile.MaxPreferredAge = Math.Max(profile.MinPreferredAge, maxPreferredAge);
        profile.MaxDistanceKm = Math.Max(1, maxDistanceKm);
        profile.LoverType = loverType.Trim();
        profile.IsEnabled = isEnabled;
        profile.IsArchived = !isEnabled;
        profile.UpdatedAt = DateTime.UtcNow;

        profile.Interests.Clear();
        foreach (var interest in SplitList(interests))
        {
            profile.Interests.Add(new DatingInterest
            {
                UserId = userId,
                Name = interest,
            });
        }

        profile.Photos.Clear();
        var index = 0;
        foreach (var photoUrl in SplitList(photoUrls))
        {
            profile.Photos.Add(new DatingPhoto
            {
                UserId = userId,
                Url = photoUrl,
                SortOrder = index++,
            });
        }

        await db.SaveChangesAsync();
        return profile;
    }

    public async Task DisableAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var profile = await db.DatingProfiles.FindAsync(userId);
        if (profile == null)
        {
            return;
        }

        profile.IsEnabled = false;
        profile.IsArchived = true;
        profile.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<DatingCandidate>> GetCandidatesAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var currentProfile = await db.DatingProfiles
            .Include(profile => profile.Interests)
            .FirstOrDefaultAsync(profile => profile.UserId == userId && profile.IsEnabled && !profile.IsArchived);

        if (currentProfile == null)
        {
            return [];
        }

        var alreadyReviewedUserIds = await db.DatingSwipes
            .Where(swipe => swipe.FromUserId == userId)
            .Select(swipe => swipe.ToUserId)
            .ToListAsync();

        var myInterests = currentProfile.Interests
            .Select(interest => interest.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var candidates = await db.DatingProfiles
            .Include(profile => profile.User)
            .Include(profile => profile.Interests)
            .Include(profile => profile.Photos.OrderBy(photo => photo.SortOrder))
            .Where(profile => profile.UserId != userId
                && profile.IsEnabled
                && !profile.IsArchived
                && !alreadyReviewedUserIds.Contains(profile.UserId))
            .OrderByDescending(profile => profile.UpdatedAt)
            .ThenBy(profile => profile.DisplayName)
            .Take(50)
            .ToListAsync();

        return candidates
            .Where(profile => PreferencesAllow(currentProfile, profile))
            .Take(30)
            .Select(profile => new DatingCandidate(
                profile,
                profile.Interests
                    .Select(interest => interest.Name)
                    .Where(myInterests.Contains)
                    .OrderBy(interest => interest)
                    .ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<DatingCandidate>> GetMatchesAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var currentProfile = await db.DatingProfiles
            .Include(profile => profile.Interests)
            .FirstOrDefaultAsync(profile => profile.UserId == userId && profile.IsEnabled && !profile.IsArchived);

        if (currentProfile == null)
        {
            return [];
        }

        var likedByMe = await db.DatingSwipes
            .Where(swipe => swipe.FromUserId == userId && PositiveActions.Contains(swipe.Action))
            .Select(swipe => swipe.ToUserId)
            .ToListAsync();

        var likedMe = await db.DatingSwipes
            .Where(swipe => swipe.ToUserId == userId && PositiveActions.Contains(swipe.Action))
            .Select(swipe => swipe.FromUserId)
            .ToListAsync();

        var matchIds = likedByMe.Intersect(likedMe).ToList();
        if (!matchIds.Any())
        {
            return [];
        }

        var myInterests = currentProfile.Interests
            .Select(interest => interest.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var profiles = await db.DatingProfiles
            .Include(profile => profile.User)
            .Include(profile => profile.Interests)
            .Include(profile => profile.Photos.OrderBy(photo => photo.SortOrder))
            .Where(profile => matchIds.Contains(profile.UserId) && profile.IsEnabled && !profile.IsArchived)
            .OrderBy(profile => profile.DisplayName)
            .ToListAsync();

        return profiles
            .Select(profile => new DatingCandidate(
                profile,
                profile.Interests
                    .Select(interest => interest.Name)
                    .Where(myInterests.Contains)
                    .OrderBy(interest => interest)
                    .ToList()))
            .ToList();
    }

    public async Task<bool> ReactAsync(Guid userId, Guid targetUserId, string action)
    {
        if (userId == targetUserId)
        {
            throw new InvalidOperationException("You cannot match with yourself.");
        }

        var normalizedAction = NormalizeAction(action);
        using var db = await _contextFactory.CreateDbContextAsync();

        var targetExists = await db.DatingProfiles.AnyAsync(profile =>
            profile.UserId == targetUserId && profile.IsEnabled && !profile.IsArchived);
        if (!targetExists)
        {
            throw new InvalidOperationException("This profile is no longer available.");
        }

        var swipe = await db.DatingSwipes.FirstOrDefaultAsync(existing =>
            existing.FromUserId == userId && existing.ToUserId == targetUserId);

        if (swipe == null)
        {
            db.DatingSwipes.Add(new DatingSwipe
            {
                FromUserId = userId,
                ToUserId = targetUserId,
                Action = normalizedAction,
                CreatedAt = DateTime.UtcNow,
            });
        }
        else
        {
            swipe.Action = normalizedAction;
            swipe.CreatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        if (!PositiveActions.Contains(normalizedAction))
        {
            return false;
        }

        return await db.DatingSwipes.AnyAsync(existing =>
            existing.FromUserId == targetUserId
            && existing.ToUserId == userId
            && PositiveActions.Contains(existing.Action));
    }

    private static IReadOnlyList<string> SplitList(string value)
    {
        return value
            .Split([',', '\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();
    }

    private static bool PreferencesAllow(DatingProfile currentProfile, DatingProfile candidateProfile)
    {
        return PreferredGendersAllow(currentProfile.PreferredGenders, candidateProfile.Gender)
            && PreferredGendersAllow(candidateProfile.PreferredGenders, currentProfile.Gender);
    }

    private static bool PreferredGendersAllow(string preferredGenders, string gender)
    {
        if (string.IsNullOrWhiteSpace(preferredGenders)
            || preferredGenders.Contains("Everyone", StringComparison.OrdinalIgnoreCase)
            || preferredGenders.Contains("Any", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return preferredGenders
            .Split([',', ';', '|'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Any(preference => PreferenceMatchesGender(preference, gender));
    }

    private static bool PreferenceMatchesGender(string preference, string gender)
    {
        if (preference.Equals(gender, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return preference.ToLowerInvariant() switch
        {
            "women" => gender.Equals("Woman", StringComparison.OrdinalIgnoreCase),
            "men" => gender.Equals("Man", StringComparison.OrdinalIgnoreCase),
            "non-binary people" => gender.Equals("Non-binary", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private static string NormalizeAction(string action)
    {
        return action?.Trim().ToLowerInvariant() switch
        {
            "pass" => "Pass",
            "like" => "Like",
            "superlike" or "super-like" => "SuperLike",
            _ => throw new InvalidOperationException("Unknown matchmaking action."),
        };
    }
}
