using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.notificationServices;


namespace ChatAndEvents.Data.EventsData.Services
{

    public class NotificationHttpService : INotificationService
    {
        private readonly HttpClient _httpClient;

        public NotificationHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task NotifyAsync(Guid userId, string title, string description)
        {
            var response = await _httpClient.PostAsync(
                $"api/Notifications?userId={userId}&title={Uri.EscapeDataString(title)}&description={Uri.EscapeDataString(description)}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Notification>> GetNotificationsAsync(Guid userId)
        {
            var notifications = await _httpClient.GetFromJsonAsync<List<Notification>>(
                $"api/Notifications/{userId}");

            return notifications ?? new List<Notification>();
        }

        public async Task DeleteAsync(int notificationId)
        {
            var response = await _httpClient.DeleteAsync($"api/Notifications/{notificationId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
