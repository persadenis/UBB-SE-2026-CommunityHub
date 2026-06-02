namespace ChatAndEvents.Data.CommunityHub.Models;

public class DatingPhoto
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public DatingProfile? DatingProfile { get; set; }

    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
