using System;
using ChatAndEvents.Data.ChatData.domain;
using ChatModule.src.view_models;
using Xunit;

namespace ChatModule.Tests
{
    public class FriendListItemViewModelTests
    {
        private readonly User testUser;
        private FriendListItemViewModel viewModel;

        public FriendListItemViewModelTests()
        {
            testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "Teo",
                AvatarUrl = "http://example.com/avatar.png",
                Status = UserStatus.Online,
                Birthday = new DateTime(2000, 1, 1)
            };

            viewModel = new FriendListItemViewModel(testUser);
        }

        [Fact]
        public void Constructor_NullUser_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FriendListItemViewModel(null!));
        }

        [Fact]
        public void Id_ReturnsUserGuid()
        {
            Assert.Equal(testUser.Id, viewModel.Id);
        }

        [Fact]
        public void Username_ReturnsUserUsername()
        {
            Assert.Equal("Teo", viewModel.Username);
        }

        [Fact]
        public void AvatarUrl_ReturnsUserAvatarUrl()
        {
            Assert.Equal("http://example.com/avatar.png", viewModel.AvatarUrl);
        }

        [Fact]
        public void AvatarInitial_WithValidName_ReturnsFirstLetterUpper()
        {
            testUser.Username = "teo";
            viewModel = new FriendListItemViewModel(testUser);
            Assert.Equal("T", viewModel.AvatarInitial);
        }

        [Fact]
        public void AvatarInitial_WithNullName_ReturnsQuestionMark()
        {
            testUser.Username = null!;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.Equal("?", viewModel.AvatarInitial);
        }

        [Fact]
        public void AvatarInitial_WithWhitespaceName_ReturnsQuestionMark()
        {
            testUser.Username = "   ";
            viewModel = new FriendListItemViewModel(testUser);
            Assert.Equal("?", viewModel.AvatarInitial);
        }

        [Fact]
        public void HasAvatar_WithUrl_ReturnsTrue()
        {
            Assert.True(viewModel.HasAvatar);
        }

        [Fact]
        public void HasAvatar_WithNullUrl_ReturnsFalse()
        {
            testUser.AvatarUrl = null;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.False(viewModel.HasAvatar);
        }

        [Fact]
        public void IsOnline_WhenOnline_ReturnsTrue()
        {
            testUser.Status = UserStatus.Online;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.True(viewModel.IsOnline);
        }

        [Fact]
        public void IsOnline_WhenNotOnline_ReturnsFalse()
        {
            testUser.Status = UserStatus.Offline;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.False(viewModel.IsOnline);
        }

        [Fact]
        public void IsBusy_WhenBusy_ReturnsTrue()
        {
            testUser.Status = UserStatus.Busy;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.True(viewModel.IsBusy);
        }

        [Fact]
        public void IsOffline_WhenOffline_ReturnsTrue()
        {
            testUser.Status = UserStatus.Offline;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.True(viewModel.IsOffline);
        }

        [Fact]
        public void StatusLabel_ReturnsEnumString()
        {
            testUser.Status = UserStatus.Busy;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.Equal("Busy", viewModel.StatusLabel);
        }

        [Fact]
        public void IsBirthdayToday_WhenTodayIsBirthday_ReturnsTrue()
        {
            testUser.Birthday = DateTime.Today;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.True(viewModel.IsBirthdayToday);
        }

        [Fact]
        public void IsBirthdayToday_WhenNoBirthdaySet_ReturnsFalse()
        {
            testUser.Birthday = null;
            viewModel = new FriendListItemViewModel(testUser);
            Assert.False(viewModel.IsBirthdayToday);
        }

        [Fact]
        public void IsBirthdayToday_WhenMonthMatchesButDayDoesNot_ReturnsFalse()
        {
            testUser.Birthday = new DateTime(2000, DateTime.Today.Month, DateTime.Today.Day).AddDays(1);
            viewModel = new FriendListItemViewModel(testUser);
            Assert.False(viewModel.IsBirthdayToday);
        }

        [Fact]
        public void IsBirthdayToday_WhenMonthDoesNotMatch_ReturnsFalse()
        {
            testUser.Birthday = DateTime.Today.AddMonths(1);
            viewModel = new FriendListItemViewModel(testUser);
            Assert.False(viewModel.IsBirthdayToday);
        }
    }
}