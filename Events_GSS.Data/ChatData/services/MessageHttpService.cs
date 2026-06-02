using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class MessageHttpService : IMessageService
    {
        private readonly HttpClient _httpClient;

        public MessageHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string?> GetCannotSendReasonAsync(Guid conversationId, Guid userId)
        {
            var response = await _httpClient.GetAsync(
                $"api/Message/{conversationId}/cannot-send-reason?userId={userId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content) || content.Trim() == "null")
            {
                return null;
            }

            return JsonSerializer.Deserialize<string?>(content, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        public async Task<List<Message>> GetMessagesAsync(Guid conversationId, Guid userId, int skip, int take)
        {
            var messages = await _httpClient.GetFromJsonAsync<List<Message>>(
                $"api/Message/{conversationId}?userId={userId}&skip={skip}&take={take}");

            return messages ?? new List<Message>();
        }

        public async Task<Message> SendMessageAsync(Guid conversationId, Guid senderId, string content, Guid? replyToId)
        {
            var requestUri =
                $"api/Message?conversationId={conversationId}&senderId={senderId}&content={Uri.EscapeDataString(content)}";

            if (replyToId.HasValue)
            {
                requestUri += $"&replyToId={replyToId.Value}";
            }

            var response = await _httpClient.PostAsync(requestUri, null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Message>()
                   ?? throw new InvalidOperationException("Failed to read message from API.");
        }

        public Task<string> PersistImageAttachmentAsync(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                throw new InvalidOperationException("Attachment file was not found.");
            }

            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
            {
                throw new InvalidOperationException("Only PNG and JPEG images are supported.");
            }

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var attachmentsDir = Path.Combine(appData, "ChatModule", "attachments");
            Directory.CreateDirectory(attachmentsDir);

            var appFolder = AppContext.BaseDirectory;
            var binAttachmentsDir = Path.Combine(appFolder, "attachments");
            Directory.CreateDirectory(binAttachmentsDir);

            var targetFileName = $"{Guid.NewGuid():N}{extension}";
            var targetPath = Path.Combine(attachmentsDir, targetFileName);
            File.Copy(sourcePath, targetPath, overwrite: false);

            var binTargetPath = Path.Combine(binAttachmentsDir, targetFileName);
            if (!File.Exists(binTargetPath))
            {
                File.Copy(sourcePath, binTargetPath, overwrite: false);
            }

            return Task.FromResult(targetPath);
        }

        public async Task EditMessageAsync(Guid messageId, Guid requesterId, string newContent)
        {
            var response = await _httpClient.PutAsync(
                $"api/Message/{messageId}?requesterId={requesterId}&newContent={Uri.EscapeDataString(newContent)}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteMessageAsync(Guid messageId, Guid requesterId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/Message/{messageId}?requesterId={requesterId}");

            response.EnsureSuccessStatusCode();
        }

        public async Task SetNicknameAsync(Guid conversationId, Guid userId, string? nickname)
        {
            var requestUri = $"api/Message/{conversationId}/nickname?userId={userId}";

            if (nickname != null)
            {
                requestUri += $"&nickname={Uri.EscapeDataString(nickname)}";
            }

            var response = await _httpClient.PutAsync(requestUri, null);
            response.EnsureSuccessStatusCode();
        }
    }
}
