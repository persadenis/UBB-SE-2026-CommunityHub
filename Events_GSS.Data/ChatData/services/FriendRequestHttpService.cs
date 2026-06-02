using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class FriendRequestHttpService : IFriendRequestService
    {
        private readonly HttpClient _httpClient;

        public FriendRequestHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendFriendRequestAsync(Guid senderUserId, Guid receiverUserId)
        {
            var response = await _httpClient.PostAsync(
                $"api/FriendRequest?senderUserId={senderUserId}&receiverUserId={receiverUserId}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> SendFriendRequestByUsernameAsync(Guid senderUserId, string receiverUsername)
        {
            var response = await _httpClient.PostAsync(
                $"api/FriendRequest/username?senderUserId={senderUserId}&username={Uri.EscapeDataString(receiverUsername)}",
                null);

            return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task AcceptFriendRequestAsync(Guid currentUserId, Guid requesterUserId)
        {
            var response = await _httpClient.PostAsync(
                $"api/FriendRequest/accept?currentUserId={currentUserId}&requesterUserId={requesterUserId}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeclineFriendRequestAsync(Guid currentUserId, Guid requesterUserId)
        {
            var response = await _httpClient.PostAsync(
                $"api/FriendRequest/decline?currentUserId={currentUserId}&requesterUserId={requesterUserId}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<User>> GetIncomingRequestsAsync(Guid currentUserId)
        {
            var requests = await _httpClient.GetFromJsonAsync<List<User>>($"api/FriendRequest/{currentUserId}");
            return requests ?? new List<User>();
        }

        public async Task<FriendStatus?> GetRelationshipStatusAsync(Guid firstUserId, Guid secondUserId)
        {
            return await _httpClient.GetFromJsonAsync<FriendStatus?>(
                $"api/FriendRequest/status?firstUserId={firstUserId}&secondUserId={secondUserId}");
        }
    }
}
