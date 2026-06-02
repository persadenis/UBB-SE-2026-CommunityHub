using System;
using System.Collections.Generic;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Web.Models;

public class MessageSearchViewModel
{
    public Guid ConversationId { get; set; }
    public Guid CurrentUserId { get; set; }
    public string Query { get; set; } = string.Empty;
    public List<Message> Results { get; set; } = new();
    public bool HasSearched { get; set; }
    public string? NoResultsMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
