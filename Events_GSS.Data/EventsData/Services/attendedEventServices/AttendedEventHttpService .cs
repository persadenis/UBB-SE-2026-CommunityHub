using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.attendedEventServices;

public class AttendedEventHttpService : IAttendedEventService
{
    private readonly HttpClient _httpClient;

    public AttendedEventHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AttendedEvent>> GetAttendedEventsAsync(Guid userId)
    {
        var events = await _httpClient.GetFromJsonAsync<List<AttendedEvent>>($"api/AttendedEvents/{userId}");
        return events ?? new List<AttendedEvent>();
    }

    public async Task<List<AttendedEvent>> GetEventsByArchiveStatusAsync(Guid userId, bool isArchived)
    {
        var events = await _httpClient.GetFromJsonAsync<List<AttendedEvent>>($"api/AttendedEvents/{userId}/archived?isArchived={isArchived}");
        return events ?? new List<AttendedEvent>();
    }

    public async Task<AttendedEvent?> GetAsync(int eventId, Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/AttendedEvents/{eventId}/{userId}");
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content) || content.Trim() == "null") return null;
        return JsonSerializer.Deserialize<AttendedEvent>(content, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public async Task AttendEventAsync(int eventId, Guid userId)
    {
        var response = await _httpClient.PostAsync($"api/AttendedEvents/{eventId}/{userId}/attend", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task LeaveEventAsync(int eventId, Guid userId)
    {
        var response = await _httpClient.DeleteAsync($"api/AttendedEvents/{eventId}/{userId}/leave");
        response.EnsureSuccessStatusCode();
    }

    public async Task SetArchivedAsync(int eventId, Guid userId, bool isArchived)
    {
        var response = await _httpClient.PutAsync($"api/AttendedEvents/{eventId}/{userId}/archive?isArchived={isArchived}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetFavouriteAsync(int eventId, Guid userId, bool isFavourite)
    {
        var response = await _httpClient.PutAsync($"api/AttendedEvents/{eventId}/{userId}/favourite?isFavourite={isFavourite}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<AttendedEvent>> GetCommonEventsAsync(Guid userId, Guid friendId)
    {
        var events = await _httpClient.GetFromJsonAsync<List<AttendedEvent>>($"api/AttendedEvents/{userId}/common/{friendId}");
        return events ?? new List<AttendedEvent>();
    }
}