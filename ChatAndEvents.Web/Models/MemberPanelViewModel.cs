using System;
using System.Collections.Generic;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Web.Models;

public class MemberPanelViewModel
{
    public Guid ConversationId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }
    public string AddMemberQuery { get; set; } = string.Empty;
    public List<MemberDisplayItem> Members { get; set; } = new();
    public List<MemberDisplayItem> BannedMembers { get; set; } = new();
    public List<User> AddMemberResults { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}
