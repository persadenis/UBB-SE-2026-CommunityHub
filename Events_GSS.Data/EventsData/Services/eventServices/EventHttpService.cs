using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.eventServices;

public class EventHttpService : IEventService
{
    private readonly HttpClient _httpClient;

    public EventHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Event>> GetAllPublicActiveEventsAsync()
    {
        var events = await _httpClient.GetFromJsonAsync<List<Event>>("api/Events");
        return events ?? new List<Event>();
    }

    public async Task<Event?> GetEventByIdAsync(int eventId)
    {
        var response = await _httpClient.GetAsync($"api/Events/{eventId}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Event>();
    }

    public async Task<int> CreateEventAsync(Event eventEntity)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Events", eventEntity);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    public async Task UpdateEventAsync(Event eventEntity)
    {
        var response = await _httpClient.PutAsJsonAsync("api/Events", eventEntity);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEventAsync(int eventId)
    {
        var response = await _httpClient.DeleteAsync($"api/Events/{eventId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Event>> GetMyEventsAsync(Guid adminId)
    {
        var events = await _httpClient.GetFromJsonAsync<List<Event>>($"api/Events/admin/{adminId}");
        return events ?? new List<Event>();
    }

    public async Task<List<Event>> SearchByTitleAsync(string title)
    {
        var events = await _httpClient.GetFromJsonAsync<List<Event>>($"api/Events/search?title={Uri.EscapeDataString(title)}");
        return events ?? new List<Event>();
    }

    public async Task<List<Event>> FilterByCategoryAsync(string category)
    {
        var events = await _httpClient.GetFromJsonAsync<List<Event>>($"api/Events/filter/category?category={Uri.EscapeDataString(category)}");
        return events ?? new List<Event>();
    }

    public async Task<List<Event>> FilterByLocationAsync(string location)
    {
        var events = await _httpClient.GetFromJsonAsync<List<Event>>($"api/Events/filter/location?location={Uri.EscapeDataString(location)}");
        return events ?? new List<Event>();
    }

    public async Task<List<Event>> FilterByDateAsync(DateTime date)
    {
        var events = await _httpClient.GetFromJsonAsync<List<Event>>($"api/Events/filter/date?date={date:O}");
        return events ?? new List<Event>();
    }

    public async Task<List<Event>> FilterByDateRangeAsync(DateTime from, DateTime to)
    {
        var events = await _httpClient.GetFromJsonAsync<List<Event>>($"api/Events/filter/daterange?from={from:O}&to={to:O}");
        return events ?? new List<Event>();
    }
}