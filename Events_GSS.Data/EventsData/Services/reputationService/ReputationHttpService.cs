using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.reputationService;

public class ReputationHttpService : IReputationService
{
    private readonly HttpClient _httpClient;

    public ReputationHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserReputationScore> GetReputationScoreAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<UserReputationScore>(
            $"api/Reputation/{userId}/score")
            ?? new UserReputationScore { UserId = userId };
    }

    public async Task<int> GetReputationPointsAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<int>($"api/Reputation/{userId}/points");
    }

    public async Task<string> GetTierAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<string>($"api/Reputation/{userId}/tier") ?? string.Empty;
    }

    public async Task<List<Achievement>> GetAchievementsAsync(Guid userId)
    {
        var achievements = await _httpClient.GetFromJsonAsync<List<Achievement>>(
            $"api/Reputation/{userId}/achievements");

        return achievements ?? new List<Achievement>();
    }

    public async Task<bool> CanPostMemoriesAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<bool>($"api/Reputation/{userId}/can-post-memories");
    }

    public async Task<bool> CanPostMessagesAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<bool>($"api/Reputation/{userId}/can-post-messages");
    }

    public async Task<bool> CanCreateEventsAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<bool>($"api/Reputation/{userId}/can-create-events");
    }

    public async Task<bool> CanAttendEventsAsync(Guid userId)
    {
        return await _httpClient.GetFromJsonAsync<bool>($"api/Reputation/{userId}/can-attend-events");
    }
}
