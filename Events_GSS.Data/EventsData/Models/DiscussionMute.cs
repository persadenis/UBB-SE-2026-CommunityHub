using System;
using System.Collections.Generic;
using System.Text;

namespace ChatAndEvents.Data.EventsData.Models;

public class DiscussionMute
{
    public int EventId { get; set; }
    public User MutedUser { get; set; } = null!;
    public Guid MutedUserId { get; set; }
    public User MutedBy { get; set; } = null!;
    public Guid MutedById { get; set; }

    public DateTime? MutedUntil { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DiscussionId { get; set; }
    public Guid UserId {  get; set; }
    public Discussion Discussion { get; internal set; }
}

