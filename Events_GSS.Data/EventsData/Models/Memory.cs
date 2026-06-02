using System;

namespace ChatAndEvents.Data.EventsData.Models
{
    public class Memory
    {
        public int MemoryId { get; set; }
        public string? PhotoPath { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public Event Event { get; set; } = null!;
        public User Author { get; set; } = null!;
        public ICollection<MemoryLike> Likes { get; set; } = new List<MemoryLike>();
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }


        public Memory(string? photoPath, string? text, DateTime createdAt)
        {
            PhotoPath = photoPath;
            Text = text;
            CreatedAt = createdAt;
            LikesCount = 0;
            IsLikedByCurrentUser = false;
        }
        public Memory() { }

        public int EventId { get; set; }
        public Guid AuthorId { get; set; }

    }
}


