using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class EventStatisticsViewModel
{
    public int EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public ParticipantOverview ParticipantOverview { get; set; } = new();

    public EngagementBreakdown EngagementBreakdown { get; set; } = new();

    public List<LeaderboardEntry> Leaderboard { get; set; } = new();

    public List<QuestAnalyticsEntry> QuestAnalytics { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
