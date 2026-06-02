using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.CommunityHub.Models;

public class CommunityPost
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CommunityId { get; set; }

    public HubCommunity? Community { get; set; }

    public Guid AuthorId { get; set; }

    public User? Author { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
