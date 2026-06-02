using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class DirectMessageHttpService : IDirectMessageService
    {
        private readonly HttpClient _httpClient;

        public DirectMessageHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Conversation> GetOrCreateAsync(Guid userId1, Guid userId2)
        {
            var response = await _httpClient.PostAsync(
                $"api/DirectMessage/conversation?userId1={userId1}&userId2={userId2}", null);

            response.EnsureSuccessStatusCode();
            
            // We deserialize the Conversation object sent back by the Controller
            return await response.Content.ReadFromJsonAsync<Conversation>() 
                   ?? throw new InvalidOperationException("Failed to read conversation from API.");
        }

        public async Task<User?> GetOtherUserAsync(Guid conversationId, Guid viewerUserId)
        {
            return await _httpClient.GetFromJsonAsync<User>(
                $"api/DirectMessage/{conversationId}/other-user?viewerUserId={viewerUserId}");
        }

        public async Task<bool> IsBlockedAsync(Guid conversationId, Guid viewerUserId)
        {
            var response = await _httpClient.GetAsync(
                $"api/DirectMessage/{conversationId}/blocked?viewerUserId={viewerUserId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            return false;
        }

        // ====================================================================
        // 🚩 WARNING: MISSING BACKEND ENDPOINTS
        // The methods below exist in the Interface, but the backend team 
        // forgot to build the endpoints for them in DirectMessageController!
        // ====================================================================

        public Task<Participant?> GetOtherParticipantAsync(Guid conversationId, Guid currentUserId)
        {
            throw new NotImplementedException("Backend Team: Please add GET api/DirectMessage/{id}/participant to the Controller.");
        }

        public Task<(Message Pinned, Message Notice)> PinMessageAsync(Guid conversationId, Guid requesterId, Guid messageId, DateTime expiresAt)
        {
            throw new NotImplementedException("Backend Team: Please add POST api/DirectMessage/{id}/pin to the Controller.");
        }

        public Task<Message> UnpinMessageAsync(Guid conversationId, Guid requesterId)
        {
            throw new NotImplementedException("Backend Team: Please add POST api/DirectMessage/{id}/unpin to the Controller.");
        }

        public Task ClearExpiredPinAsync(Guid conversationId, Guid pinnedMessageId)
        {
            throw new NotImplementedException("Backend Team: Please add DELETE api/DirectMessage/{id}/pin to the Controller.");
        }
    }
}