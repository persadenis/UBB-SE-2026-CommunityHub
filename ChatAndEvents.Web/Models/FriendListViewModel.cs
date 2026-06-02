using System.Collections.Generic;

namespace ChatAndEvents.Web.Models;

public class FriendListViewModel
{
    public List<FriendListItemViewModel> Friends { get; set; } = new();

    public string FriendUsernameInput { get; set; } = string.Empty;

    public string? FriendActionMessage { get; set; }

    public bool HasFriends => Friends.Count > 0;
}
