// <copyright file="AnnouncementReadReceipt.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.Models;

/// <summary>
/// Refers to the users that have read a specific announcement.
/// </summary>
public class AnnouncementReadReceipt
{
    public int AnnouncementId { get; set; }

    public ChatAndEvents.Data.ChatData.domain.User? User { get; set; }

    public DateTime ReadAt { get; set; }
    public Guid UserId {  get; set; }
    public Announcement Announcement { get; set; }
}
