using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class ConversationListHttpService : IConversationListService
    {
        private readonly HttpClient _httpClient;

        public ConversationListHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Conversation>> GetAllAsync(Guid userId)
        {
            var conversations = await _httpClient.GetFromJsonAsync<List<Conversation>>(
                $"api/ConversationList/{userId}/all");

            return conversations ?? new List<Conversation>();
        }

        public async Task<Conversation?> GetByIdAsync(Guid conversationId)
        {
            var response = await _httpClient.GetAsync($"api/ConversationList/{conversationId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Conversation>();
        }

        public async Task<List<Conversation>> GetDmsAsync(Guid userId)
        {
            var conversations = await _httpClient.GetFromJsonAsync<List<Conversation>>(
                $"api/ConversationList/{userId}/dms");

            return conversations ?? new List<Conversation>();
        }

        public async Task<List<Conversation>> GetFavouritesAsync(Guid userId)
        {
            var conversations = await _httpClient.GetFromJsonAsync<List<Conversation>>(
                $"api/ConversationList/{userId}/favourites");

            return conversations ?? new List<Conversation>();
        }

        public async Task<List<Conversation>> GetGroupsAsync(Guid userId)
        {
            var conversations = await _httpClient.GetFromJsonAsync<List<Conversation>>(
                $"api/ConversationList/{userId}/groups");

            return conversations ?? new List<Conversation>();
        }

        public async Task<Message?> GetLastMessageAsync(Guid conversationId)
        {
            var response = await _httpClient.GetAsync(
                $"api/ConversationList/{conversationId}/last-message");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Message>();
        }

        public async Task<List<Conversation>> GetUnreadAsync(Guid userId)
        {
            var conversations = await _httpClient.GetFromJsonAsync<List<Conversation>>(
                $"api/ConversationList/{userId}/unread");

            return conversations ?? new List<Conversation>();
        }

        public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId)
        {
            var response = await _httpClient.GetAsync(
                $"api/ConversationList/{conversationId}/unread-count?userId={userId}");

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }

            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<List<Conversation>> SearchAsync(Guid userId, string query)
        {
            var conversations = await _httpClient.GetFromJsonAsync<List<Conversation>>(
                $"api/ConversationList/{userId}/search?query={Uri.EscapeDataString(query)}");

            return conversations ?? new List<Conversation>();
        }

        public async Task SetFavouriteAsync(Guid conversationId, Guid userId, bool isFavourite)
        {
            var response = await _httpClient.PutAsync(
                $"api/ConversationList/{conversationId}/favourite?userId={userId}&isFavourite={isFavourite}",
                null);

            response.EnsureSuccessStatusCode();
        }
    }
}

