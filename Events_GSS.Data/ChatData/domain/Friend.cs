using System;

namespace ChatAndEvents.Data.ChatData.domain
{
    public class Friend
    {
        public Guid Id { get; set; }
        public Guid UserId1 { get; set; }
        public Guid UserId2 { get; set; }
        public FriendStatus Status { get; set; }
        public bool IsMatch { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User User1 { get; set; }
        public User User2 { get; set; }
    }
}
