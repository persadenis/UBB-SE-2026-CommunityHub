using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.userServices;

namespace ChatModule.src.HttpService
{
    public class UserHttpService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly CurrentUserContext _currentUserContext;

        public UserHttpService(HttpClient httpClient, CurrentUserContext currentUserContext)
        {
            _httpClient = httpClient;
            _currentUserContext = currentUserContext;
        }

        public async Task<User> GetCurrentUser()
        {
            if (_currentUserContext.UserId == Guid.Empty)
            {
                throw new InvalidOperationException("Current user id is not set.");
            }

            return await _httpClient.GetFromJsonAsync<User>($"api/User/{_currentUserContext.UserId}")
                   ?? throw new InvalidOperationException("Failed to read current user from API.");
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            var response = await _httpClient.GetAsync($"api/User/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<User>();
        }

        public List<User> GetFriends(Guid userId)
        {
            var friends = _httpClient.GetFromJsonAsync<List<User>>(
                $"api/User/{userId}/friends").GetAwaiter().GetResult();

            return friends ?? new List<User>();
        }

        public List<User> SearchFriends(Guid userId, string name)
        {
            var friends = _httpClient.GetFromJsonAsync<List<User>>(
                $"api/User/{userId}/search-friends?name={Uri.EscapeDataString(name)}").GetAwaiter().GetResult();

            return friends ?? new List<User>();
        }

        public async Task<bool> IsAttending(Event currentEvent)
        {
            if (_currentUserContext.UserId == Guid.Empty)
            {
                return false;
            }

            var response = await _httpClient.GetAsync(
                $"api/User/{currentEvent.EventId}/attending?userId={_currentUserContext.UserId}");

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public bool IsAdmin(Event currentEvent)
        {
            if (_currentUserContext.UserId == Guid.Empty)
            {
                return false;
            }

            var response = _httpClient.GetAsync(
                $"api/User/{currentEvent.EventId}/admin?userId={_currentUserContext.UserId}").GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = response.Content.ReadFromJsonAsync<bool>().GetAwaiter().GetResult();
            return result;
        }
    }
}

