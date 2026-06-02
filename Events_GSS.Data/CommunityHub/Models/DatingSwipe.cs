namespace ChatAndEvents.Data.CommunityHub.Models;

public class DatingSwipe
{
    public int Id { get; set; }

    public Guid FromUserId { get; set; }

    public Guid ToUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
