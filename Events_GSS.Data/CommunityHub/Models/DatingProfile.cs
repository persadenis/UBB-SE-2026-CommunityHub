using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.CommunityHub.Models;

public class DatingProfile
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public bool IsEnabled { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string PreferredGenders { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string DatingBio { get; set; } = string.Empty;

    public int MinPreferredAge { get; set; } = 18;

    public int MaxPreferredAge { get; set; } = 99;

    public int MaxDistanceKm { get; set; } = 50;

    public string LoverType { get; set; } = string.Empty;

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DatingInterest> Interests { get; set; } = new List<DatingInterest>();

    public ICollection<DatingPhoto> Photos { get; set; } = new List<DatingPhoto>();
}
