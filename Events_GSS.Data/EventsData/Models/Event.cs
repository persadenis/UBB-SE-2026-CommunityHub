using System;

namespace ChatAndEvents.Data.EventsData.Models;

public class Event
{
    public int EventId { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsPublic { get; set; }
    public string? Description { get; set; }
    public int? MaximumPeople { get; set; }
    public string? EventBannerPath { get; set; }
    public int? SlowModeSeconds { get; set; }
    public int EnrolledCount { get; set; } = 0;
    public Guid? AdminId { get; set; }
    public int? CategoryId { get; set; }
    // Navigation properties 
    public Category? Category { get; set; }
    public User? Admin { get; set; }
    public ICollection<AttendedEvent> Attendees { get; set; } = new List<AttendedEvent>();
    public ICollection<Memory> Memories { get; set; } = new List<Memory>();
    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    public ICollection<Quest> Quests { get; set; } = new List<Quest>();
}
