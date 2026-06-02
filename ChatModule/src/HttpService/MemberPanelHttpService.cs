using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class MemberPanelHttpService : IMemberPanelService
    {
        private readonly HttpClient _httpClient;

        public MemberPanelHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Participant>> GetMembersAsync(Guid conversationId)
        {
            var members = await _httpClient.GetFromJsonAsync<List<Participant>>(
                $"api/MemberPanel/{conversationId}/members");
            
            return members ?? new List<Participant>();
        }

        public async Task<List<Participant>> GetBannedMembersAsync(Guid conversationId)
        {
            var bannedMembers = await _httpClient.GetFromJsonAsync<List<Participant>>(
                $"api/MemberPanel/{conversationId}/banned");
            
            return bannedMembers ?? new List<Participant>();
        }

        public async Task<List<User>> SearchUsersToAddAsync(Guid conversationId, string query)
        {
            // Save a network trip if the query is empty
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<User>();
            }

            var users = await _httpClient.GetFromJsonAsync<List<User>>(
                $"api/MemberPanel/{conversationId}/search?query={Uri.EscapeDataString(query)}");
            
            return users ?? new List<User>();
        }

        public async Task<User?> GetUserAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"api/MemberPanel/user/{userId}");
            
            // This safely handles the NotFound() response from your controller
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<User>();
            }

            return null;
        }
    }
}