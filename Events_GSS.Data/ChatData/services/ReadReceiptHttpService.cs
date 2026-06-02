using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class ReadReceiptHttpService : IReadReceiptService
    {
        private readonly HttpClient _httpClient;

        public ReadReceiptHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task MarkAsReadAsync(Guid conversationId, Guid userId, Guid messageId)
        {
            var response = await _httpClient.PostAsync(
                $"api/ReadReceipt/{conversationId}/mark?userId={userId}&messageId={messageId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task MarkLatestAsReadAsync(Guid conversationId, Guid userId)
        {
            var response = await _httpClient.PostAsync(
                $"api/ReadReceipt/{conversationId}/mark-latest?userId={userId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Participant>> GetReadReceiptsAsync(Guid conversationId, Guid messageId)
        {
            var receipts = await _httpClient.GetFromJsonAsync<List<Participant>>(
                $"api/ReadReceipt/{conversationId}/{messageId}");

            return receipts ?? new List<Participant>();
        }

        public async Task<int> GetReadByCountAsync(Guid conversationId, Guid messageId)
        {
            return await _httpClient.GetFromJsonAsync<int>(
                $"api/ReadReceipt/{conversationId}/{messageId}/count");
        }

        public async Task<int> GetReadByOthersCountAsync(Guid conversationId, Guid messageId, Guid currentUserId)
        {
            return await _httpClient.GetFromJsonAsync<int>(
                $"api/ReadReceipt/{conversationId}/{messageId}/others-count?currentUserId={currentUserId}");
        }

        public async Task<Guid?> GetLastReadMessageAsync(Guid conversationId, Guid userId)
        {
            var response = await _httpClient.GetAsync(
                $"api/ReadReceipt/{conversationId}/last-read?userId={userId}");

            response.EnsureSuccessStatusCode();
            return await ReadNullableJsonAsync<Guid>(response);
        }

        public async Task<List<Participant>> GetParticipantsAsync(Guid conversationId)
        {
            var participants = await _httpClient.GetFromJsonAsync<List<Participant>>(
                $"api/ReadReceipt/{conversationId}/participants");

            return participants ?? new List<Participant>();
        }

        public async Task<DateTime?> GetLastReadTimestampAsync(Guid conversationId, Guid userId)
        {
            var response = await _httpClient.GetAsync(
                $"api/ReadReceipt/{conversationId}/timestamp?userId={userId}");

            response.EnsureSuccessStatusCode();
            return await ReadNullableJsonAsync<DateTime>(response);
        }

        public async Task<List<string>> GetReaderUsernamesAsync(Guid conversationId, Guid messageId, Guid? excludeUserId = null)
        {
            var requestUri = $"api/ReadReceipt/{conversationId}/{messageId}/readers";
            
            if (excludeUserId.HasValue)
            {
                requestUri += $"?excludeUserId={excludeUserId.Value}";
            }

            var usernames = await _httpClient.GetFromJsonAsync<List<string>>(requestUri);

            return usernames ?? new List<string>();
        }

        private static async Task<T?> ReadNullableJsonAsync<T>(HttpResponseMessage response)
            where T : struct
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content) || content.Trim() == "null")
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
