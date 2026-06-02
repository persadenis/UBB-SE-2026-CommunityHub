using System;

namespace ChatAndEvents.Data.ChatData.domain
{
    public class Participant
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public ParticipantRole Role { get; set; }
        public Guid? LastReadMessageId { get; set; }
        public DateTime? TimeoutUntil { get; set; }
        public bool IsFavourite { get; set; }
        public bool IsNew { get; set; }
        public string? Nickname { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; }
        public User User { get; set; }
        public Message? LastReadMessage { get; set; }
    }
}
