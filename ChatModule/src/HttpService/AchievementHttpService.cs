using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.achievementServices;

namespace ChatModule.src.HttpService
{
    public class AchievementHttpService : IAchievementService
    {
        private readonly HttpClient _httpClient;

        public AchievementHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Achievement>> GetUserAchievementsAsync(Guid userId)
        {
            var achievements = await _httpClient.GetFromJsonAsync<List<Achievement>>(
                $"api/Achievement/{userId}");

            return achievements ?? new List<Achievement>();
        }

        public async Task CheckAndAwardAchievementsAsync(Guid userId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Achievement/{userId}/check", null);

            response.EnsureSuccessStatusCode();
        }
    }
}
