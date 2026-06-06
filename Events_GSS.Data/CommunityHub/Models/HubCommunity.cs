using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.CommunityHub.Models;

public class HubCommunity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = "General";

    public string? BannerUrl { get; set; }

    public Guid OwnerId { get; set; }

    public User? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CommunityMembership> Members { get; set; } = new List<CommunityMembership>();

    public ICollection<CommunityPost> Posts { get; set; } = new List<CommunityPost>();
}
