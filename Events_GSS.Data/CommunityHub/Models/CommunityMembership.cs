using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.CommunityHub.Models;

public class CommunityMembership
{
    public Guid CommunityId { get; set; }

    public HubCommunity? Community { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public bool IsAdmin { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
