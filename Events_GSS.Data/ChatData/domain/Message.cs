using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAndEvents.Data.ChatData.domain
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid? UserId { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ReplyToId { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public MessageType MessageType { get; set; }
        public Guid? ParentMessageId { get; set; }
        [NotMapped] public string? SenderUsername { get; set; }
        [NotMapped] public string? SenderAvatarUrl { get; set; }
        [NotMapped]
        public string SenderInitial => !string.IsNullOrWhiteSpace(SenderUsername)
            ? SenderUsername.Substring(0, 1).ToUpperInvariant()
            : "?";
        [NotMapped] public Dictionary<string, int> ReactionCounts { get; set; } = new ();
        [NotMapped] public bool IsMine { get; set; }
        [NotMapped] public int ReadByCount { get; set; }
        [NotMapped] public string? ReadReceiptLabel { get; set; }
        [NotMapped] public bool ShowUnreadSeparator { get; set; }
        public string? AttachmentImagePath { get; set; }
        [NotMapped] public string? ReplyPreviewText { get; set; }
        [NotMapped] public string? ReplyPreviewSender { get; set; }
        [NotMapped] public string? ReplyPreviewContent { get; set; }
        [NotMapped] public bool CanEdit { get; set; }
        [NotMapped] public bool CanDelete { get; set; }
        [NotMapped] public bool CanPin { get; set; }
        public DateTime? PinExpiresAt { get; set; }

        //Navigation properties
        public Conversation Conversation { get; set; }
        public User? User { get; set; }
        public Message? ReplyTo { get; set; }
        public Message? ParentMessage { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? LinkPreviewTitle { get; set; }
        public string? LinkPreviewDesc { get; set; }
    }
}
