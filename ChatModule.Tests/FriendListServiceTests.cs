using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ChatModule.Tests
{
    public class FriendListServiceTests
    {
        private readonly Mock<IFriendRepository> mockFriendRepo;
        private readonly Mock<IUserRepository> mockUserRepo;
        private readonly FriendListService service;
        private readonly Guid currentUserId;

        public FriendListServiceTests()
        {
            mockFriendRepo = new Mock<IFriendRepository>();
            mockUserRepo = new Mock<IUserRepository>();
            currentUserId = Guid.NewGuid();

            service = new FriendListService(mockFriendRepo.Object, mockUserRepo.Object);
        }

        [Fact]
        public void Constructor_NullFriendRepo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FriendListService(null!, mockUserRepo.Object));
        }

        [Fact]
        public void Constructor_NullUserRepo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FriendListService(mockFriendRepo.Object, null!));
        }

        [Fact]
        public async Task GetFriendsAsync_NoFriends_ReturnsEmptyList()
        {
            mockFriendRepo.Setup(r => r.GetAcceptedFriendsAsync(currentUserId))
                .ReturnsAsync(new List<Friend>());

            var result = await service.GetFriendsAsync(currentUserId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFriendsAsync_WithFriends_ReturnsUserList()
        {
            var friendId = Guid.NewGuid();
            var relations = new List<Friend>
            {
                new Friend { UserId1 = currentUserId, UserId2 = friendId }
            };
            var mockUser = new User { Id = friendId, Username = "TestFriend" };

            mockFriendRepo.Setup(r => r.GetAcceptedFriendsAsync(currentUserId)).ReturnsAsync(relations);
            mockUserRepo.Setup(r => r.GetByIdAsync(friendId)).ReturnsAsync(mockUser);

            var result = await service.GetFriendsAsync(currentUserId);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetFriendsAsync_WhenTargetIsUserId2_ReturnsCorrectUser()
        {
            var friendId = Guid.NewGuid();
            var relations = new List<Friend>
            {
                new Friend { UserId1 = friendId, UserId2 = currentUserId }
            };
            var mockUser = new User { Id = friendId, Username = "TestFriend" };

            mockFriendRepo.Setup(r => r.GetAcceptedFriendsAsync(currentUserId)).ReturnsAsync(relations);
            mockUserRepo.Setup(r => r.GetByIdAsync(friendId)).ReturnsAsync(mockUser);

            var result = await service.GetFriendsAsync(currentUserId);

            Assert.Single(result); 
        }

        [Fact]
        public async Task GetFriendsAsync_WhenUserNotFound_SkipsUser()
        {
            var friendId = Guid.NewGuid();
            var relations = new List<Friend>
            {
                new Friend { UserId1 = currentUserId, UserId2 = friendId }
            };

            mockFriendRepo.Setup(r => r.GetAcceptedFriendsAsync(currentUserId)).ReturnsAsync(relations);

            mockUserRepo.Setup(r => r.GetByIdAsync(friendId)).ReturnsAsync((User?)null);

            var result = await service.GetFriendsAsync(currentUserId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task RemoveFriendAsync_CallsDeleteFriendship()
        {
            var targetFriendId = Guid.NewGuid();

            await service.RemoveFriendAsync(currentUserId, targetFriendId);

            mockFriendRepo.Verify(r => r.DeleteFriendshipAsync(currentUserId, targetFriendId), Times.Once);
        }
    }
}