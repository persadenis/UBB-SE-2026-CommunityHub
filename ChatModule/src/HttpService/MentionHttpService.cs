using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class MentionHttpService : IMentionService
    {
        private readonly HttpClient _httpClient;

        public MentionHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<User>> GetCandidatesAsync(Guid conversationId, string query)
        {
            var users = await _httpClient.GetFromJsonAsync<List<User>>(
                $"api/Mention/{conversationId}/candidates?query={Uri.EscapeDataString(query)}");

            return users ?? new List<User>();
        }

        public async Task<List<Guid>> ExtractMentionedUserIdsAsync(Guid conversationId, string content)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/Mention/{conversationId}/extract",
                content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Guid>>() ?? new List<Guid>();
        }
    }
}

