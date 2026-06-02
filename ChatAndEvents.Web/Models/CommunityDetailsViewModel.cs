using ChatAndEvents.Data.CommunityHub.Models;

namespace ChatAndEvents.Web.Models;

public class CommunityDetailsViewModel
{
    public HubCommunity Community { get; set; } = new();

    public bool IsCurrentUserMember { get; set; }

    public bool IsCurrentUserAdmin { get; set; }
}
