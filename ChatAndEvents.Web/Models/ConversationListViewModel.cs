using System;
using System.Collections.Generic;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Web.Models
{
    public class ConversationListViewModel
    {
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
        public string ActiveTab { get; set; } = "All";
        public string SearchQuery { get; set; } = string.Empty;
        
        public const string AllTab = "All";
        public const string DirectMessagesTab = "Direct Messages";
        public const string GroupsTab = "Groups";
        public const string FavoritesTab = "Favorites";
        public const string UnreadTab = "Unread";
    }
}