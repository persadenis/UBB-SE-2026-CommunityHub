using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class BlockHttpService : IBlockService
    {
        private readonly HttpClient _httpClient;

        public BlockHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task BlockUserAsync(Guid blockerUserId, Guid targetUserId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Block?blockerUserId={blockerUserId}&targetUserId={targetUserId}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task UnblockUserAsync(Guid blockerUserId, Guid targetUserId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/Block?blockerUserId={blockerUserId}&targetUserId={targetUserId}");

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<User>> GetBlockedUsersAsync(Guid targetUserId)
        {
            var users = await _httpClient.GetFromJsonAsync<List<User>>($"api/Block/{targetUserId}");
            return users ?? new List<User>();
        }

        public async Task<bool> IsBlockedAsync(Guid blockerId, Guid targetId)
        {
            var response = await _httpClient.GetAsync($"api/Block/check?blockerId={blockerId}&targetId={targetId}");
            return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<bool> CheckIfBlockedAsync(Guid blockerUserId, Guid targetUserId)
        {
            return await IsBlockedAsync(blockerUserId, targetUserId);
        }
    }
}
