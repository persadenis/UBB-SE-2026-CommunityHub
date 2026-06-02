using System;
using ChatAndEvents.Data.ChatData.domain;
using Xunit;

namespace ChatModule.Tests
{
    public class FriendTests
    {
        private readonly Friend friend;
        private readonly Guid testId;
        private readonly DateTime testDate;

        public FriendTests()
        {
            testId = Guid.NewGuid();
            testDate = DateTime.UtcNow;
            friend = new Friend
            {
                Id = testId,
                UserId1 = Guid.NewGuid(),
                UserId2 = Guid.NewGuid(),
                Status = FriendStatus.Accepted,
                IsMatch = true,
                CreatedAt = testDate
            };
        }

        [Fact]
        public void Id_SetAndGet_ReturnsCorrectValue()
        {
            Assert.Equal(testId, friend.Id);
        }

        [Fact]
        public void Status_SetAndGet_ReturnsCorrectValue()
        {
            friend.Status = FriendStatus.Blocked;
            Assert.Equal(FriendStatus.Blocked, friend.Status);
        }

        [Fact]
        public void IsMatch_SetAndGet_ReturnsCorrectValue()
        {
            friend.IsMatch = false;
            Assert.False(friend.IsMatch);
        }

        [Fact]
        public void CreatedAt_ReturnsCorrectValue()
        {
            Assert.Equal(testDate, friend.CreatedAt);
        }
    }
}