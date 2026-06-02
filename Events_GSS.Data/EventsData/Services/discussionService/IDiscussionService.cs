using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.discussionService;

public interface IDiscussionService
{
    // ── Messages ──────────────────────────────────────────────
    Task<List<DiscussionMessage>> GetMessagesAsync(int eventId, Guid userId);
    Task CreateMessageAsync(string? text, string? mediaPath, int eventId, Guid userId, int? replyToId);
    Task DeleteMessageAsync(int messageId, Guid userId, int eventId);

    // ── Reactions ─────────────────────────────────────────────
    Task ReactAsync(int messageId, Guid userId, string emoji);
    Task RemoveReactionAsync(int messageId, Guid userId);

    // ── Mutes ─────────────────────────────────────────────────
    Task MuteUserAsync(int eventId, Guid targetUserId, DateTime? muteUntil, Guid adminUserId);
    Task UnmuteUserAsync(int eventId, Guid targetUserId, Guid adminUserId);

    // ── Slow Mode ───────────────────────────────────────────────
    Task SetSlowModeAsync(int eventId, int? seconds, Guid adminUserId);
    Task<int?> GetSlowModeSecondsAsync(int eventId);

    // ── Participants ─────────────────────────────────────────────────────
    Task<List<User>> GetEventParticipantsAsync(int eventId);
}
