using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;

public class EventStatisticsHttpService : IEventStatisticsService
{
    private readonly HttpClient _httpClient;

    public EventStatisticsHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ParticipantOverview> GetParticipantOverviewAsync(int eventId)
    {
        return await _httpClient.GetFromJsonAsync<ParticipantOverview>(
            $"api/EventStatistics/{eventId}/participants")
            ?? new ParticipantOverview();
    }

    public async Task<EngagementBreakdown> GetEngagementBreakdownAsync(int eventId)
    {
        return await _httpClient.GetFromJsonAsync<EngagementBreakdown>(
            $"api/EventStatistics/{eventId}/engagement")
            ?? new EngagementBreakdown();
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int eventId)
    {
        var leaderboard = await _httpClient.GetFromJsonAsync<List<LeaderboardEntry>>(
            $"api/EventStatistics/{eventId}/leaderboard");

        return leaderboard ?? new List<LeaderboardEntry>();
    }

    public async Task<List<QuestAnalyticsEntry>> GetQuestAnalyticsAsync(int eventId)
    {
        var analytics = await _httpClient.GetFromJsonAsync<List<QuestAnalyticsEntry>>(
            $"api/EventStatistics/{eventId}/quests");

        return analytics ?? new List<QuestAnalyticsEntry>();
    }
}
