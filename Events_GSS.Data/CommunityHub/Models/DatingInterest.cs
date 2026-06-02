namespace ChatAndEvents.Data.CommunityHub.Models;

public class DatingInterest
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public DatingProfile? DatingProfile { get; set; }

    public string Name { get; set; } = string.Empty;
}
