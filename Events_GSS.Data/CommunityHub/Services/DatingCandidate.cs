using ChatAndEvents.Data.CommunityHub.Models;

namespace ChatAndEvents.Data.CommunityHub.Services;

public sealed record DatingCandidate(
    DatingProfile Profile,
    IReadOnlyList<string> SharedInterests);
