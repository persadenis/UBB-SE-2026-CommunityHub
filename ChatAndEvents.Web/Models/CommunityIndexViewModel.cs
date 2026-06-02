using ChatAndEvents.Data.CommunityHub.Services;

namespace ChatAndEvents.Web.Models;

public class CommunityIndexViewModel
{
    public string? Query { get; set; }

    public string? Category { get; set; }

    public IReadOnlyList<string> Categories { get; set; } = [];

    public IReadOnlyList<CommunitySearchResult> Communities { get; set; } = [];
}
