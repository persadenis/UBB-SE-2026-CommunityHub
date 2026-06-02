using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ChatModule.Tests
{
    public class ProfileServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IFriendRepository> _mockFriendRepo;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockFriendRepo = new Mock<IFriendRepository>();

            _profileService = new ProfileService(
                _mockUserRepo.Object,
                _mockFriendRepo.Object
            );
        }

        [Fact]
        public async Task GetProfileAsync_UserExists_ReturnsUserWithCorrectId()
        {
            var targetUserId = Guid.NewGuid();
            _mockUserRepo.Setup(r => r.GetByIdAsync(targetUserId))
                         .ReturnsAsync(new User { Id = targetUserId });

            var result = await _profileService.GetProfileAsync(targetUserId);

            Assert.Equal(targetUserId, result?.Id);
        }


        [Fact]
        public async Task GetAllUsersAsync_NoQuery_FiltersOutViewer()
        {
            var viewerId = Guid.NewGuid();
            var allUsers = new List<User> { new User { Id = viewerId }, new User { Id = Guid.NewGuid() } };
            _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(allUsers);

            var result = await _profileService.GetAllUsersAsync(viewerId, null);

            Assert.DoesNotContain(result, u => u.Id == viewerId);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithQuery_CallsSearchRepo()
        {
            var viewerId = Guid.NewGuid();
            _mockUserRepo.Setup(r => r.SearchByUsernameAsync("test")).ReturnsAsync(new List<User>());

            await _profileService.GetAllUsersAsync(viewerId, "test");

            _mockUserRepo.Verify(r => r.SearchByUsernameAsync("test"), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_UserNotFound_DoesNotUpdate()
        {
            var userId = Guid.NewGuid();
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            await _profileService.UpdateProfileAsync(userId, "bio", "url", DateTime.Now);

            _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProfileAsync_UserExists_UpdatesBio()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User { Id = userId, Bio = "Old" };
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

            await _profileService.UpdateProfileAsync(userId, "NewBio", "url", DateTime.Now);

            Assert.Equal("NewBio", existingUser.Bio);
        }

        [Fact]
        public async Task UpdateProfileAsync_UserExists_CallsUpdateRepo()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User { Id = userId };
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

            await _profileService.UpdateProfileAsync(userId, "bio", "url", DateTime.Now);

            _mockUserRepo.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        }


        [Fact]
        public async Task UpdateStatusAsync_UserExists_SetsNewStatus()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User { Id = userId, Status = UserStatus.Offline };
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

            await _profileService.UpdateStatusAsync(userId, UserStatus.Online);

            Assert.Equal(UserStatus.Online, existingUser.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_UserNotFound_DoesNothing()
        {
            var userId = Guid.NewGuid();
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            await _profileService.UpdateStatusAsync(userId, UserStatus.Online);

            _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetMutualFriendsAsync_ReturnsCorrectCount()
        {
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();
            var mutualId = Guid.NewGuid();
            _mockFriendRepo.Setup(r => r.GetMutualFriendIdentifiersAsync(user1, user2))
                           .ReturnsAsync(new List<Guid> { mutualId });
            _mockUserRepo.Setup(r => r.GetByIdAsync(mutualId)).ReturnsAsync(new User());

            var result = await _profileService.GetMutualFriendsAsync(user1, user2);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetMutualFriendsAsync_MapsCorrectUserObject()
        {
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();
            var mutualId = Guid.NewGuid();
            var expectedUser = new User { Id = mutualId, Username = "Friend" };

            _mockFriendRepo.Setup(r => r.GetMutualFriendIdentifiersAsync(user1, user2))
                           .ReturnsAsync(new List<Guid> { mutualId });
            _mockUserRepo.Setup(r => r.GetByIdAsync(mutualId)).ReturnsAsync(expectedUser);

            var result = await _profileService.GetMutualFriendsAsync(user1, user2);

            Assert.Equal("Friend", result[0].Username);
        }
    }
}