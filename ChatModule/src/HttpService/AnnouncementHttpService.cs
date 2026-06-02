using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.announcementServices;

namespace ChatModule.src.HttpService
{
    public class AnnouncementHttpService : IAnnouncementService
    {
        private readonly HttpClient _httpClient;

        public AnnouncementHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Announcement>> GetAnnouncementsAsync(int eventId, Guid userId)
        {
            var announcements = await _httpClient.GetFromJsonAsync<List<Announcement>>(
                $"api/Announcements/{eventId}?userId={userId}");

            return announcements ?? new List<Announcement>();
        }

        public async Task CreateAnnouncementAsync(string announcementMessage, int eventId, Guid userId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Announcements?message={Uri.EscapeDataString(announcementMessage)}&eventId={eventId}&userId={userId}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAnnouncementAsync(int announcementId, string newAnnouncementMessage, Guid userId, int eventId)
        {
            var response = await _httpClient.PutAsync(
                $"api/Announcements/{announcementId}?newMessage={Uri.EscapeDataString(newAnnouncementMessage)}&userId={userId}&eventId={eventId}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAnnouncementAsync(int announcementId, Guid userId, int eventId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/Announcements/{announcementId}?userId={userId}&eventId={eventId}");

            response.EnsureSuccessStatusCode();
        }

        public async Task PinAnnouncementAsync(int announcementId, int eventId, Guid userId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Announcements/{announcementId}/pin?eventId={eventId}&userId={userId}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> MarkAsReadAsync(int announcementId, Guid userId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Announcements/{announcementId}/read?userId={userId}",
                null);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var marked = await response.Content.ReadFromJsonAsync<bool>();
            return marked;
        }

        public async Task<(List<AnnouncementReadReceipt> Readers, int TotalParticipants)> GetReadReceiptsAsync(
            int announcementId,
            int eventId,
            Guid userId)
        {
            var response = await _httpClient.GetFromJsonAsync<ReadReceiptsResponse>(
                $"api/Announcements/{announcementId}/receipts?eventId={eventId}&userId={userId}");

            if (response == null)
            {
                return (new List<AnnouncementReadReceipt>(), 0);
            }

            return (response.Readers ?? new List<AnnouncementReadReceipt>(), response.Total);
        }

        public async Task AddOrUpdateReactAsync(int announcementId, Guid userId, string emoji)
        {
            var response = await _httpClient.PutAsync(
                $"api/Announcements/{announcementId}/react?userId={userId}&emoji={Uri.EscapeDataString(emoji)}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveReactionAsync(int announcementId, Guid userId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/Announcements/{announcementId}/react?userId={userId}");

            response.EnsureSuccessStatusCode();
        }

        public async Task<Dictionary<int, int>> GetUnreadCountsForUserAsync(Guid userId)
        {
            var counts = await _httpClient.GetFromJsonAsync<Dictionary<int, int>>(
                $"api/Announcements/unread/{userId}");

            return counts ?? new Dictionary<int, int>();
        }

        public async Task<List<User>> GetAllParticipantsAsync(int eventId)
        {
            var participants = await _httpClient.GetFromJsonAsync<List<User>>(
                $"api/Announcements/{eventId}/participants");

            return participants ?? new List<User>();
        }

        public async Task ToggleReactionAsync(int announcementId, Guid userId, string emoji)
        {
            var response = await _httpClient.PostAsync(
                $"api/Announcements/{announcementId}/react?userId={userId}&emoji={Uri.EscapeDataString(emoji)}",
                null);

            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> MarkAsReadIfNeededAsync(int announcementId, Guid userId, bool isAlreadyRead)
        {
            if (isAlreadyRead)
            {
                return false;
            }

            return await MarkAsReadAsync(announcementId, userId);
        }

        public async Task<List<User>> GetNonReadersAsync(int announcementId, int eventId)
        {
            var nonReaders = await _httpClient.GetFromJsonAsync<List<User>>(
                $"api/Announcements/{announcementId}/nonreaders?eventId={eventId}");

            return nonReaders ?? new List<User>();
        }

        public void AttachReactions(
            List<Announcement> announcements,
            List<(int AnnouncementId, AnnouncementReaction Reaction)> reactions)
        {
            var grouped = reactions
                .GroupBy(reaction => reaction.AnnouncementId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Reaction).ToList());

            foreach (var announcement in announcements)
            {
                if (grouped.TryGetValue(announcement.Id, out var listOfReactions))
                {
                    announcement.Reactions = listOfReactions;
                }
            }
        }

        private sealed class ReadReceiptsResponse
        {
            public List<AnnouncementReadReceipt>? Readers { get; set; }

            public int Total { get; set; }
        }
    }
}
