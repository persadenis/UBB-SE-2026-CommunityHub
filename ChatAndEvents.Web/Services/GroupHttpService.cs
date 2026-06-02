using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Web.Services
{
    public class GroupHttpService : IGroupService
    {
        private readonly HttpClient _httpClient;

        public GroupHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Conversation> CreateGroupAsync(
            Guid creatorId,
            string title,
            string? iconUrl,
            List<Guid> memberIds)
        {
            var payload = new { creatorId, title, iconUrl, memberIds };
            var response = await _httpClient.PostAsJsonAsync("api/group", payload);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Conversation>()
                ?? throw new InvalidOperationException("Empty response from group endpoint.");
        }

        public Task UpdateGroupInfoAsync(Guid conversationId, Guid requesterId, string? newTitle, string? newIconUrl)
            => throw new NotImplementedException();

        public Task LeaveGroupAsync(Guid conversationId, Guid userId)
            => throw new NotImplementedException();

        public Task PinMessageAsync(Guid conversationId, Guid requesterId, Guid messageId)
            => throw new NotImplementedException();

        public Task UnpinMessageAsync(Guid conversationId, Guid requesterId)
            => throw new NotImplementedException();

        public Task PostEventNoticeAsync(Guid conversationId, Guid adminId, string eventTitle, DateTime eventDate)
            => throw new NotImplementedException();
    }
}