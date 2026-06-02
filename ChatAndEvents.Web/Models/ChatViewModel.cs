using System;
using System.Collections.Generic;
using ChatAndEvents.Data.ChatData.domain;
namespace ChatAndEvents.Web.Models
{
    public class ChatViewModel
    {
        public Guid ConversationId { get; set; }
        public Guid CurrentUserId { get; set; }
        public string ConversationTitle { get; set; } = string.Empty;
        
        public List<Message> Messages { get; set; } = new List<Message>();
        public Message? PinnedMessage { get; set; }
        
        public string MessageInput { get; set; } = string.Empty;
        
        public bool IsInputDisabled { get; set; }
        public string? InputDisabledReason { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
