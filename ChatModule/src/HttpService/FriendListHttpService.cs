using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class FriendListHttpService : IFriendListService
    {
        private readonly HttpClient _httpClient;

        public FriendListHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<User>> GetFriendsAsync(Guid targetUserId)
        {
            var friends = await _httpClient.GetFromJsonAsync<List<User>>(
                $"api/FriendList/{targetUserId}");

            return friends ?? new List<User>();
        }

        public async Task RemoveFriendAsync(Guid currentUserId, Guid targetFriendId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/FriendList?currentUserId={currentUserId}&targetFriendId={targetFriendId}");

            response.EnsureSuccessStatusCode();
        }
    }
}