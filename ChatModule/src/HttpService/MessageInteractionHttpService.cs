using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class MessageInteractionHttpService : IMessageInteractionService
    {
        private readonly HttpClient _httpClient;

        public MessageInteractionHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task ReactToMessageAsync(Guid messageId, Guid userId, string emoji)
        {
            var encodedEmoji = Uri.EscapeDataString(emoji);
            
            var response = await _httpClient.PostAsync(
                $"api/MessageInteraction/{messageId}/react?userId={userId}&emoji={encodedEmoji}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveReactionAsync(Guid messageId, Guid userId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/MessageInteraction/{messageId}/react?userId={userId}");

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Message>> GetReactionsAsync(Guid messageId)
        {
            var reactions = await _httpClient.GetFromJsonAsync<List<Message>>(
                $"api/MessageInteraction/{messageId}/reactions");

            return reactions ?? new List<Message>();
        }

        public async Task<string?> BuildReplyPreviewAsync(Guid messageId)
        {
            var response = await _httpClient.GetAsync(
                $"api/MessageInteraction/{messageId}/reply-preview");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }

        public async Task<(string Sender, string Content)?> BuildReplyPreviewPartsAsync(Guid messageId)
        {
            
            var response = await _httpClient.GetAsync(
                $"api/MessageInteraction/{messageId}/reply-preview-parts");

            if (response.IsSuccessStatusCode)
            {
                var previewParts = await response.Content.ReadFromJsonAsync<PreviewPartsDto>();
                if (previewParts != null)
                {
                    return (previewParts.Sender, previewParts.Content);
                }
            }

            return null;
        }
        
        private class PreviewPartsDto
        {
            public string Sender { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }
    }
}