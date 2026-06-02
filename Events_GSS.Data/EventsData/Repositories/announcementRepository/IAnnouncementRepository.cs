// <copyright file="IAnnouncementRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.Repositories.announcementRepository;

using ChatAndEvents.Data.EventsData.Models;

/// <summary>
/// Provides data access operations for announcements, including creation, updates,
/// deletion, reactions, pinning, and read receipt tracking.
/// </summary>
public interface IAnnouncementRepository
{
    // ── Announcements ─────────────────────────────────────────

    /// <summary>
    /// Retrieves all announcements for a specific event, including user read state.
    /// </summary>
    Task<List<Announcement>> GetAnnouncementsByEventAsync(int eventId, Guid userId);

    /// <summary>
    /// Adds a new announcement to an event and returns its generated identifier.
    /// </summary>
    Task<int> AddAnnouncementAsync(Announcement announcement, int eventId, Guid userId);

    /// <summary>
    /// Updates the message content of an existing announcement.
    /// </summary>
    Task UpdateAnnouncementAsync(int announcementId, string newMessage);

    /// <summary>
    /// Deletes an announcement from the system.
    /// </summary>
    Task DeleteAnnouncementAsync(int selectedEvent);

    /// <summary>
    /// Retrieves a single announcement by its identifier.
    /// </summary>
    Task<Announcement?> GetAnnouncementByIdAsync(int announcementId);

    // ── Pinning ─────────────────────────────────────────

    /// <summary>
    /// Pins a specific announcement within an event.
    /// </summary>
    Task PinAsync(int announcementId);

    /// <summary>
    /// Removes the pinned status from all announcements in an event.
    /// </summary>
    Task UnpinAnnouncementAsync(int eventId);

    // ── Read Receipts ─────────────────────────────────────────

    /// <summary>
    /// Marks an announcement as read by a specific user.
    /// </summary>
    Task InsertReadReceiptAsync(int announcementId, Guid userId);

    Task<bool> HasUserReadAsync(int announcementId, Guid userId);

    /// <summary>
    /// Retrieves all read receipts for a given announcement.
    /// </summary>
    Task<List<AnnouncementReadReceipt>> GetReadReceiptsAsync(int announcementId);

    /// <summary>
    /// Gets the total number of participants for an event.
    /// </summary>
    Task<int> GetTotalParticipantsAsync(int eventId);

    /// <summary>
    /// Retrieves unread announcement counts grouped by event for a user.
    /// </summary>
    Task<Dictionary<int, int>> GetUnreadCountsForUserAsync(Guid userId);

    /// <summary>
    /// Retrieves all users participating in an event.
    /// </summary>
    Task<List<User>> GetAllParticipantsAsync(int eventId);

    // ── Reactions ─────────────────────────────────────────

    /// <summary>
    /// Adds a new reaction or updates an existing reaction for an announcement.
    /// </summary>
    Task UpdateReactionAsync(int announcementId, Guid userId, string emoji);

    Task InsertReactionAsync(int announcementId, Guid userId, string emoji);

    /// <summary>
    /// Removes a user's reaction from an announcement.
    /// </summary>
    Task RemoveReactionAsync(int announcementId, Guid userId);

    /// <summary>
    /// Gets the reaction emoji a user has given to an announcement, if any.
    /// </summary>
    Task<string?> GetUserReactionAsync(int announcementId, Guid userId);

    Task<List<(int AnnouncementId, AnnouncementReaction Reaction)>> GetReactionsAsync(
    List<int> announcementIds);
}