using System;
using System.Net.Http;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;

namespace ChatAndEvents.Data.ChatData.services
{
    public class ModerationHttpService : IModerationService
    {
        private readonly HttpClient _httpClient;

        public ModerationHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task BanMemberAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Moderation/{conversationId}/ban?adminId={adminId}&targetId={targetId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task UnbanMemberAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Moderation/{conversationId}/unban?adminId={adminId}&targetId={targetId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task TimeoutMemberAsync(Guid conversationId, Guid adminId, Guid targetId, TimeSpan duration)
        {
            var durationMinutes = (int)duration.TotalMinutes;

            var response = await _httpClient.PostAsync(
                $"api/Moderation/{conversationId}/timeout?adminId={adminId}&targetId={targetId}&durationMinutes={durationMinutes}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveTimeoutAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Moderation/{conversationId}/untimeout?adminId={adminId}&targetId={targetId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task PromoteMemberAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Moderation/{conversationId}/promote?adminId={adminId}&targetId={targetId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task DemoteMemberAsync(Guid conversationId, Guid adminId, Guid targetId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Moderation/{conversationId}/demote?adminId={adminId}&targetId={targetId}", null);

            response.EnsureSuccessStatusCode();
        }

        public async Task AddMemberAsync(Guid conversationId, Guid adminId, Guid newUserId)
        {
            var response = await _httpClient.PostAsync(
                $"api/Moderation/{conversationId}/add-member?adminId={adminId}&newUserId={newUserId}", null);

            response.EnsureSuccessStatusCode();
        }
    }
}