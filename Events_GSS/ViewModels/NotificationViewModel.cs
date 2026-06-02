using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.userServices;

namespace Events_GSS.ViewModels
{
    public class NotificationViewModel : INotifyPropertyChanged
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set { _isLoading = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Notification> _notifications = new();
        public ObservableCollection<Notification> Notifications
        {
            get { return _notifications; }
            set { _notifications = value; OnPropertyChanged(); }
        }

        public NotificationViewModel(INotificationService notificationService, IUserService userService)
        {
            _userService = userService;
            _notificationService = notificationService;
        }

        public async Task LoadAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var currentUser = await _userService.GetCurrentUser();
                var notifications = await _notificationService.GetNotificationsAsync(currentUser.UserId);
                Notifications = new ObservableCollection<Notification>(notifications);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not load notifications: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DeleteAsync(Notification notification)
        {
            await _notificationService.DeleteAsync(notification.Id);
            Notifications.Remove(notification);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
