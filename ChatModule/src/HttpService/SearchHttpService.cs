using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.services
{
    public class SearchHttpService : ISearchService
    {
        private readonly HttpClient _httpClient;

        public SearchHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<Message>> SearchMessagesAsync(Guid conversationId, Guid userId, string query)
        {
            var requestUri = $"api/Search/messages?conversationId={conversationId}&userId={userId}";
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                requestUri += $"&query={Uri.EscapeDataString(query)}";
            }

            var messages = await _httpClient.GetFromJsonAsync<List<Message>>(requestUri);
            
            return messages ?? new List<Message>();
        }

        public async Task<List<User>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<User>();
            }

            var requestUri = $"api/Search/users?query={Uri.EscapeDataString(query)}";

            var users = await _httpClient.GetFromJsonAsync<List<User>>(requestUri);
            
            return users ?? new List<User>();
        }

        public async Task<List<User>> SearchUsersForAddMemberAsync(Guid conversationId, string query)
        {
            var requestUri = $"api/Search/members?conversationId={conversationId}";
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                requestUri += $"&query={Uri.EscapeDataString(query)}";
            }

            var users = await _httpClient.GetFromJsonAsync<List<User>>(requestUri);
            
            return users ?? new List<User>();
        }
    }
}