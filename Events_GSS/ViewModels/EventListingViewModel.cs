using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.eventServices;

namespace Events_GSS.ViewModels
{
    public class EventListingViewModel : INotifyPropertyChanged
    {
        // 1. Safely holding the Service Interface
        private readonly IEventService _eventService;
        
        public event Action? CreateEventRequested;
        public event Action<Event>? EventDetailsRequested;

        // The UI binds to this list
        public ObservableCollection<Event> Events { get; } = new ObservableCollection<Event>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        // 2. The Constructor now strictly demands the Service, NOT the Repository
        public EventListingViewModel(IEventService eventService)
        {
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        public async Task LoadEventsAsync()
        {
            IsLoading = true;
            try
            {
                // 3. We now call the Service method instead of the DB Repository directly!
                var events = await _eventService.GetAllPublicActiveEventsAsync();
                
                Events.Clear();
                foreach (var ev in events)
                {
                    Events.Add(ev);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load events: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void RequestCreateEvent() => CreateEventRequested?.Invoke();
        public void RequestEventDetails(Event selectedEvent) => EventDetailsRequested?.Invoke(selectedEvent);
    }
}