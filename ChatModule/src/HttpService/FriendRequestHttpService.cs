using System;
using System.Collections.Generic;
using System.Net.Http;
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
            // The controller uses [FromQuery], so we put the IDs in the URL.
            // (Note: No body is sent, so we pass 'null' for the content)
            var response = await _httpClient.PostAsync(
                $"api/FriendRequest?senderUserId={senderUserId}&receiverUserId={receiverUserId}", null);
            
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> SendFriendRequestByUsernameAsync(Guid senderUserId, string receiverUsername)
        {
            var response = await _httpClient.PostAsync(
                $"api/FriendRequest/username?senderUserId={senderUserId}&username={Uri.EscapeDataString(receiverUsername)}", null);
            
            if (response.IsSuccessStatusCode)
            {
                // The controller returns Ok(result) where result is a boolean
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            return false;
        }

        public async Task AcceptFriendRequestAsync(Guid currentUserId, Guid requesterUserId)
        {
            var response = await _httpClient.PostAsync(
                $"api/FriendRequest/accept?currentUserId={currentUserId}&requesterUserId={requesterUserId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeclineFriendRequestAsync(Guid currentUserId, Guid requesterUserId)
        {
            var response = await _httpClient.PostAsync(
                $"api/FriendRequest/decline?currentUserId={currentUserId}&requesterUserId={requesterUserId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<User>> GetIncomingRequestsAsync(Guid currentUserId)
        {
            var requests = await _httpClient.GetFromJsonAsync<List<User>>(
                $"api/FriendRequest/{currentUserId}");
            
            return requests ?? new List<User>();
        }

        public async Task<FriendStatus?> GetRelationshipStatusAsync(Guid firstUserId, Guid secondUserId)
        {
            var status = await _httpClient.GetFromJsonAsync<FriendStatus?>(
                $"api/FriendRequest/status?firstUserId={firstUserId}&secondUserId={secondUserId}");
            
            return status;
        }
    }
}
