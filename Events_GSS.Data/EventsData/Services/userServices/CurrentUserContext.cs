using System;

namespace ChatAndEvents.Data.EventsData.Services.userServices;

public class CurrentUserContext
{
    public CurrentUserContext(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}
