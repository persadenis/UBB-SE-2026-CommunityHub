using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ChatAndEvents.Data.EventsData.Messaging;

public class ReputationMessage : ValueChangedMessage<ReputationAction>
{
    public Guid UserId { get; }
    public int? EventId { get; }

    public ReputationMessage(Guid userId, ReputationAction action, int? eventId = null)
        : base(action)
    {
        UserId = userId;
        EventId = eventId;
    }
}
