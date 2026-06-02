using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.discussionService;

public class DiscussionHttpService : IDiscussionService
{
    private readonly HttpClient _httpClient;

    public DiscussionHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<DiscussionMessage>> GetMessagesAsync(int eventId, Guid userId)
    {
        var messages = await _httpClient.GetFromJsonAsync<List<DiscussionMessage>>(
            $"api/Discussion/{eventId}?userId={userId}");

        return messages ?? new List<DiscussionMessage>();
    }

    public async Task CreateMessageAsync(string? text, string? mediaPath, int eventId, Guid userId, int? replyToId)
    {
        var url = $"api/Discussion?eventId={eventId}&userId={userId}";
        url += text != null ? $"&text={Uri.EscapeDataString(text)}" : "";
        url += mediaPath != null ? $"&mediaPath={Uri.EscapeDataString(mediaPath)}" : "";
        url += replyToId.HasValue ? $"&replyToId={replyToId.Value}" : "";

        var response = await _httpClient.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMessageAsync(int messageId, Guid userId, int eventId)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/Discussion/{messageId}?userId={userId}&eventId={eventId}");

        response.EnsureSuccessStatusCode();
    }

    public async Task ReactAsync(int messageId, Guid userId, string emoji)
    {
        var response = await _httpClient.PostAsync(
            $"api/Discussion/{messageId}/react?userId={userId}&emoji={Uri.EscapeDataString(emoji)}",
            null);

        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveReactionAsync(int messageId, Guid userId)
    {
        var response = await _httpClient.DeleteAsync(
            $"api/Discussion/{messageId}/react?userId={userId}");

        response.EnsureSuccessStatusCode();
    }

    public async Task MuteUserAsync(int eventId, Guid targetUserId, DateTime? muteUntil, Guid adminUserId)
    {
        var url = $"api/Discussion/{eventId}/mute?targetUserId={targetUserId}&adminUserId={adminUserId}";
        url += muteUntil.HasValue ? $"&muteUntil={muteUntil.Value:O}" : "";

        var response = await _httpClient.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnmuteUserAsync(int eventId, Guid targetUserId, Guid adminUserId)
    {
        var response = await _httpClient.PostAsync(
            $"api/Discussion/{eventId}/unmute?targetUserId={targetUserId}&adminUserId={adminUserId}",
            null);

        response.EnsureSuccessStatusCode();
    }

    public async Task SetSlowModeAsync(int eventId, int? seconds, Guid adminUserId)
    {
        var url = $"api/Discussion/{eventId}/slowmode?adminUserId={adminUserId}";
        url += seconds.HasValue ? $"&seconds={seconds.Value}" : "";

        var response = await _httpClient.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<int?> GetSlowModeSecondsAsync(int eventId)
    {
        var response = await _httpClient.GetAsync($"api/Discussion/{eventId}/slowmode");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content) || content.Trim() == "null")
        {
            return null;
        }

        return JsonSerializer.Deserialize<int>(content, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public async Task<List<User>> GetEventParticipantsAsync(int eventId)
    {
        var participants = await _httpClient.GetFromJsonAsync<List<User>>(
            $"api/Discussion/{eventId}/participants");

        return participants ?? new List<User>();
    }
}
