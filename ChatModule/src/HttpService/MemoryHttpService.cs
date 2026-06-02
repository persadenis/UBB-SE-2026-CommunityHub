using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;

namespace ChatAndEvents.Data.EventsData.Services;

public class MemoryHttpService : IMemoryService
{
    private readonly HttpClient _httpClient;

    public MemoryHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Memory>> GetByEventAsync(Event forEvent, User currentUser)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/Memories/{forEvent.EventId}")
        {
            Content = JsonContent.Create(currentUser)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<Memory>>() ?? new List<Memory>();
    }

    public async Task<List<string>> GetOnlyPhotosAsync(Event eve)
    {
        var photos = await _httpClient.GetFromJsonAsync<List<string>>(
            $"api/Memories/{eve.EventId}/photos");

        return photos ?? new List<string>();
    }

    public async Task<List<Memory>> FilterByMyMemoriesAsync(Event eve, User user)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/Memories/{eve.EventId}/mine")
        {
            Content = JsonContent.Create(user)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<Memory>>() ?? new List<Memory>();
    }

    public async Task<List<Memory>> OrderByDateAsync(Event eve, User user, bool ascending)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/Memories/{eve.EventId}/ordered?ascending={ascending}")
        {
            Content = JsonContent.Create(user)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<Memory>>() ?? new List<Memory>();
    }

    public async Task AddAsync(Event eve, User user, string? photoPath, string? text)
    {
        var url = $"api/Memories?eventId={eve.EventId}";
        url += photoPath != null ? $"&photoPath={Uri.EscapeDataString(photoPath)}" : "";
        url += text != null ? $"&text={Uri.EscapeDataString(text)}" : "";

        var response = await _httpClient.PostAsJsonAsync(url, user);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Memory memory, User user)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Memories/{memory.MemoryId}")
        {
            Content = JsonContent.Create(user)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task ToggleLikeAsync(Memory memory, User user)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"api/Memories/{memory.MemoryId}/like",
            user);

        response.EnsureSuccessStatusCode();
    }

    public async Task<int> GetLikesCountAsync(int memoryId)
    {
        return await _httpClient.GetFromJsonAsync<int>($"api/Memories/{memoryId}/likes");
    }

    public bool IsOwnMemory(Memory memory, User currentUser)
    {
        return memory.Author?.UserId == currentUser.UserId;
    }

    public bool CanDelete(Memory memory, User currentUser)
    {
        bool isAuthor = memory.Author?.UserId == currentUser.UserId;
        bool isEventAdmin = memory.Event?.Admin?.UserId == currentUser.UserId;
        return isAuthor || isEventAdmin;
    }

    public bool CanLike(Memory memory, User currentUser)
    {
        return memory.Author?.UserId != currentUser.UserId;
    }
}
