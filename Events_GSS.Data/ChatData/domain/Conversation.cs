using System;

namespace ChatAndEvents.Data.ChatData.domain
{
    public class Conversation
    {
        public Guid Id { get; set; }

        public ConversationType Type { get; set; }

        public string? Title { get; set; }

        public string? IconUrl { get; set; }

        public Guid CreatedBy { get; set; }

        public Guid? PinnedMessageId { get; set; }

        public string LastMessagePreview { get; set; } = string.Empty;

        public DateTime? LastMessageAt { get; set; }

        public int UnreadCount { get; set; }

        public bool HasUnread => UnreadCount > 0;

        // Navigation properties
        public User Creator { get; set; }
        public Message PinnedMessage { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<Participant> Participants { get; set; }
    }
}
