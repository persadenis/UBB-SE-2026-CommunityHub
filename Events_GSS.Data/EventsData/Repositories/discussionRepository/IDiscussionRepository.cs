using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChatAndEvents.Data.EventsData.Repositories.discussionRepository;

public interface IDiscussionRepository
{
    // ── Messages ──────────────────────────────────────────────
    Task<List<DiscussionMessage>> GetByEventAsync(int eventId, Guid currentUserId);
    Task<DiscussionMessage?> GetByIdAsync(int messageId);
    Task<int> AddAsync(DiscussionMessage message , int eventId, Guid userId, int? replyToId);
    Task DetachRepliesAsync(int messageId);
    Task DeleteAsync(int messageId);
    Task<DateTime?> GetLastUserMessageDateAsync(int eventId, Guid userId);

    // ── Reactions ─────────────────────────────────────────────
    Task AddReactionAsync(int messageId, Guid userId, string emoji);
    Task RemoveReactionAsync(int messageId, Guid userId);

    Task UpdateReactionAsync(int messageId, Guid userId, string emoji);
    Task<DiscussionReaction?> GetReactionAsync(int messageId, Guid userId);
    Task<List<DiscussionReaction>> GetReactionsAsync(int messageId);

    // ── Mutes ─────────────────────────────────────────────────
    Task<DiscussionMute?> GetMuteAsync(int eventId, Guid userId);
    Task UnmuteAsync(int eventId, Guid userId);

    Task DeleteExistingMuteAsync(int eventId, Guid userId);

    Task InsertMuteAsync(DiscussionMute mute);

    // -─ Slow Mode ───────────────────────────────────────────────-
    Task SetSlowModeAsync(int eventId, int? seconds);

    // ── Participants ─────────────────────────────────────────────────────
    // <summary>
    // Used for the @mention lookup when posting messages. Returns all users who have participated in the discussion (posted a message or reaction).
    // </summary>
    Task<List<ChatAndEvents.Data.EventsData.Models.User>> GetEventParticipantsAsync(int eventId);
}

