using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;

namespace Events_GSS.ViewModels
{
    public class MyEventsViewModel : INotifyPropertyChanged
    {
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        private readonly IAttendedEventService _attendedEventService;


        public event Action<Event>? EventDetailsRequested;

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

        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty;
            set
            {
                if (_isEmpty != value)
                {
                    _isEmpty = value;
                    OnPropertyChanged();
                }
            }
        }

        public MyEventsViewModel(IEventService eventService, IUserService userService, IAttendedEventService attendedEventService)
        {
            _eventService = eventService;
            _userService = userService;
            _attendedEventService = attendedEventService;
        }

        public async Task LoadMyEventsAsync()
        {
            IsLoading = true;
            try
            {
                var currentUser = await _userService.GetCurrentUser();

                // Events where user is admin
                var adminEvents = await _eventService.GetMyEventsAsync(currentUser.UserId);

                // Events where user has joined
                var attendedEvents = await _attendedEventService.GetAttendedEventsAsync(currentUser.UserId);
                var joinedEvents = attendedEvents.Select(ae => ae.Event).ToList();

                // Merge, deduplicate by EventId
                var allEvents = adminEvents
                    .Union(joinedEvents, EventIdComparer.Instance)
                    .OrderBy(e => e.StartDateTime)
                    .ToList();

                Events.Clear();
                foreach (var ev in allEvents)
                    Events.Add(ev);

                IsEmpty = Events.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load my events: {ex}");
                IsEmpty = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void RequestEventDetails(Event selectedEvent) =>
            EventDetailsRequested?.Invoke(selectedEvent);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public class EventIdComparer : IEqualityComparer<Event>
        {
            public static readonly EventIdComparer Instance = new();
            public bool Equals(Event? x, Event? y) => x?.EventId == y?.EventId;
            public int GetHashCode(Event obj) => obj.EventId.GetHashCode();
        }
    }
}