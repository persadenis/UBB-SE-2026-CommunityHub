using ChatAndEvents.Data.CommunityHub.Models;

namespace ChatAndEvents.Data.CommunityHub.Services;

public sealed record CommunitySearchResult(
    HubCommunity Community,
    int MemberCount,
    bool IsCurrentUserMember);
