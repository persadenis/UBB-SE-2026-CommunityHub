using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ChatAndEvents.Web.Services
{
    public class SearchHttpService : ISearchService
    {
        private readonly HttpClient _httpClient;

        public SearchHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<User>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<User>();

            var response = await _httpClient.GetAsync(
                $"api/search/users?query={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<User>>()
                ?? new List<User>();
        }

        public async Task<List<Message>> SearchMessagesAsync(Guid conversationId, Guid userId, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Message>();

            var response = await _httpClient.GetAsync(
                $"api/search/messages?conversationId={conversationId}&userId={userId}&query={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Message>>()
                ?? new List<Message>();
        }

        public async Task<List<User>> SearchUsersForAddMemberAsync(Guid conversationId, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<User>();

            var response = await _httpClient.GetAsync(
                $"api/search/users/add-member?conversationId={conversationId}&query={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<User>>()
                ?? new List<User>();
        }
    }
}