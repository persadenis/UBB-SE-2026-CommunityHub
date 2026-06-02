using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using System.ComponentModel;

public class EventDetailViewModel
{
    private readonly IAttendedEventService _attendedService;
    public Event SelectedEvent { get; }
    
    public event Action? BackRequested;
    public event Action<Event>? StatisticsRequested;

    // Now it takes the event AND the service
    public EventDetailViewModel(Event selectedEvent, IAttendedEventService attendedService)
    {
        SelectedEvent = selectedEvent;
        _attendedService = attendedService;
    }

    public void RequestBack() => BackRequested?.Invoke();
    public void RequestStatistics() => StatisticsRequested?.Invoke(SelectedEvent);
    
    // You could move the "Join/Leave" logic here later!
}
