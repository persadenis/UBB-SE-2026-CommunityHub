using System.Collections.Generic;

namespace ChatAndEvents.Web.Models;

public class FriendRequestsViewModel
{
    public List<FriendListItemViewModel> IncomingRequests { get; set; } = new();

    public string? RequestActionMessage { get; set; }

    public bool HasIncomingRequests => IncomingRequests.Count > 0;
}
