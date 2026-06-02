using System;

namespace ChatAndEvents.Data.EventsData.Models;

public class MemoryLike
{
    public int MemoryId { get; set; }

    public Guid UserId { get; set; }

    public Memory Memory { get; set; } = null!;

    public User User { get; set; } = null!;
}
