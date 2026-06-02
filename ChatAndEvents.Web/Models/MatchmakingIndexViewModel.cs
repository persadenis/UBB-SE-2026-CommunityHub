using ChatAndEvents.Data.CommunityHub.Models;
using ChatAndEvents.Data.CommunityHub.Services;

namespace ChatAndEvents.Web.Models;

public class MatchmakingIndexViewModel
{
    public DatingProfile? Profile { get; set; }

    public IReadOnlyList<DatingCandidate> Candidates { get; set; } = [];

    public IReadOnlyList<DatingCandidate> Matches { get; set; } = [];

    public ISet<Guid> FriendUserIds { get; set; } = new HashSet<Guid>();

    public bool HasEnabledProfile => Profile?.IsEnabled == true && Profile.IsArchived == false;

    public DatingCandidate? CurrentCandidate => Candidates.FirstOrDefault();

    public bool IsFriend(Guid userId) => FriendUserIds.Contains(userId);
}
