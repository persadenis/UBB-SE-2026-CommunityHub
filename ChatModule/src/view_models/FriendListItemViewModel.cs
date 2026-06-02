using System;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatModule.src.view_models
{
    public class FriendListItemViewModel
    {
        public Guid Id { get; }
        public string Username { get; }
        public string? AvatarUrl { get; }
        public string AvatarInitial { get; }
        public bool HasAvatar { get; }
        public bool IsOnline { get; }
        public bool IsBusy { get; }
        public bool IsOffline { get; }
        public string StatusLabel { get; }
        public bool IsBirthdayToday { get; }

        public FriendListItemViewModel(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Id = user.Id;
            Username = user.Username;
            AvatarUrl = user.AvatarUrl;
            AvatarInitial = string.IsNullOrWhiteSpace(user.Username)
                ? "?"
                : user.Username.Substring(0, 1).ToUpperInvariant();
            HasAvatar = !string.IsNullOrWhiteSpace(user.AvatarUrl);

            IsOnline = user.Status == UserStatus.Online;
            IsBusy = user.Status == UserStatus.Busy;
            IsOffline = user.Status == UserStatus.Offline;

            StatusLabel = user.Status.ToString();

            if (user.Birthday.HasValue)
            {
                var today = DateTime.Today;
                var isSameMonth = user.Birthday.Value.Month == today.Month;
                var isSameDay = user.Birthday.Value.Day == today.Day;
                IsBirthdayToday = isSameMonth && isSameDay;
            }
            else
            {
                IsBirthdayToday = false;
            }
        }
    }
}
