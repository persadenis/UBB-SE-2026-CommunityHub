using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.services
{
    public class ProfileHttpService : IProfileService
    {
        private readonly HttpClient _httpClient;

        public ProfileHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<User>> GetAllUsersAsync(Guid viewerUserId, string? searchQuery)
        {
            var requestUri = $"api/Profile?viewerUserId={viewerUserId}";

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
            }

            var users = await _httpClient.GetFromJsonAsync<List<User>>(requestUri);
            return users ?? new List<User>();
        }

        public async Task<List<User>> GetMutualFriendsAsync(Guid userId1, Guid userId2)
        {
            var mutualFriends = await _httpClient.GetFromJsonAsync<List<User>>(
                $"api/Profile/mutual?userId1={userId1}&userId2={userId2}");

            return mutualFriends ?? new List<User>();
        }

        public async Task<User?> GetProfileAsync(Guid targetUserId)
        {
            var response = await _httpClient.GetAsync($"api/Profile/{targetUserId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task UpdateProfileAsync(Guid userId, string? bio, string? avatarUrl, DateTime? birthday)
        {
            var queryParameters = new List<string>();

            if (bio != null)
            {
                queryParameters.Add($"bio={Uri.EscapeDataString(bio)}");
            }

            if (avatarUrl != null)
            {
                queryParameters.Add($"avatarUrl={Uri.EscapeDataString(avatarUrl)}");
            }

            if (birthday.HasValue)
            {
                queryParameters.Add($"birthday={Uri.EscapeDataString(birthday.Value.ToString("O"))}");
            }

            var requestUri = $"api/Profile/{userId}";
            if (queryParameters.Count > 0)
            {
                requestUri += "?" + string.Join("&", queryParameters);
            }

            var response = await _httpClient.PutAsync(requestUri, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateStatusAsync(Guid userId, UserStatus status)
        {
            var response = await _httpClient.PutAsync($"api/Profile/{userId}/status?status={status}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
