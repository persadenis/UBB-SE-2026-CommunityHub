using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class GroupHttpService : IGroupService
    {
        private readonly HttpClient _httpClient;

        public GroupHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Conversation> CreateGroupAsync(Guid creatorId, string title, string? iconUrl, List<Guid> memberIds)
        {
            // Because the Controller expects memberIds in the Body, we use PostAsJsonAsync
            var response = await _httpClient.PostAsJsonAsync(
                $"api/Group?creatorId={creatorId}&title={Uri.EscapeDataString(title)}&iconUrl={Uri.EscapeDataString(iconUrl ?? "")}", 
                memberIds ?? new List<Guid>());

            response.EnsureSuccessStatusCode();

            // We read the created conversation back from the server
            return await response.Content.ReadFromJsonAsync<Conversation>()
                   ?? throw new InvalidOperationException("Failed to read the created group from the API.");
        }

        public async Task UpdateGroupInfoAsync(Guid conversationId, Guid requesterId, string? newTitle, string? newIconUrl)
        {
            var titleParam = newTitle != null ? $"&newTitle={Uri.EscapeDataString(newTitle)}" : "";
            var iconParam = newIconUrl != null ? $"&newIconUrl={Uri.EscapeDataString(newIconUrl)}" : "";

            var response = await _httpClient.PutAsync(
                $"api/Group/{conversationId}?requesterId={requesterId}{titleParam}{iconParam}", 
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task LeaveGroupAsync(Guid conversationId, Guid userId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Group/{conversationId}/leave?userId={userId}", 
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task PinMessageAsync(Guid conversationId, Guid requesterId, Guid messageId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Group/{conversationId}/pin?requesterId={requesterId}&messageId={messageId}", 
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task UnpinMessageAsync(Guid conversationId, Guid requesterId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Group/{conversationId}/unpin?requesterId={requesterId}", 
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task PostEventNoticeAsync(Guid conversationId, Guid adminId, string eventTitle, DateTime eventDate)
        {
            // We use :O to format the DateTime securely for the URL string
            var response = await _httpClient.PostAsync(
                $"api/Group/{conversationId}/event-notice?adminId={adminId}&eventTitle={Uri.EscapeDataString(eventTitle)}&eventDate={eventDate:O}", 
                null);

            response.EnsureSuccessStatusCode();
        }
    }
}