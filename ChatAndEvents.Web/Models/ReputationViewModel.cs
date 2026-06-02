using System.Collections.Generic;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class ReputationViewModel
{
    public string UserName { get; set; } = string.Empty;

    public int ReputationPoints { get; set; }

    public string CurrentTier { get; set; } = "Newcomer";

    public List<Achievement> Achievements { get; set; } = new();

    public bool IsLoading { get; set; }

    public string? ErrorMessage { get; set; }
}
