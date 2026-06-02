namespace ChatAndEvents.Data.EventsData.Models;

using System;
using System.Collections.Generic;

public class Discussion
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateCreated { get; set; }
    public bool IsClosed { get; set; }

    // --- Navigation Properties ---

    // Discuția aparține unui eveniment
    public required Event AssociatedEvent { get; set; }

    // Discuția este creată de un utilizator
    public required User Creator { get; set; }

    // O discuție conține o listă de mesaje
    public List<DiscussionMessage> Messages { get; set; } = new();
    public List<DiscussionReaction> Reactions { get; set; } = new();
    public List<DiscussionMute> Mutes { get; set; } = new();
}