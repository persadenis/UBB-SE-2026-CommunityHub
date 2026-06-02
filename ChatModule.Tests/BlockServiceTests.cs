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
    public class BlockServiceTests
    {
        private readonly Mock<IFriendRepository> mockFriendRepo;
        private readonly Mock<IUserRepository> mockUserRepo;
        private readonly BlockService service;
        private readonly Guid blockerId;
        private readonly Guid targetId;

        public BlockServiceTests()
        {
            mockFriendRepo = new Mock<IFriendRepository>();
            mockUserRepo = new Mock<IUserRepository>();
            blockerId = Guid.NewGuid();
            targetId = Guid.NewGuid();

            service = new BlockService(mockFriendRepo.Object, mockUserRepo.Object);
        }

        [Fact]
        public void Constructor_NullFriendRepo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BlockService(null!, mockUserRepo.Object));
        }

        [Fact]
        public void Constructor_NullUserRepo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new BlockService(mockFriendRepo.Object, null!));
        }

        // --- BlockUserAsync Tests ---

        [Fact]
        public async Task BlockUserAsync_NoPriorRelation_CreatesNewBlockedRelation()
        {
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync((Friend?)null);

            await service.BlockUserAsync(blockerId, targetId);

            mockFriendRepo.Verify(r => r.CreateFriendshipAsync(It.Is<Friend>(f => f.Status == FriendStatus.Blocked)), Times.Once);
        }

        [Fact]
        public async Task BlockUserAsync_PriorRelationAccepted_SetsMatchStatusTrue()
        {
            var relation = new Friend { Status = FriendStatus.Accepted };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            await service.BlockUserAsync(blockerId, targetId);

            mockFriendRepo.Verify(r => r.SetMatchStatusAsync(blockerId, targetId, true), Times.Once);
        }

        [Fact]
        public async Task BlockUserAsync_PriorRelationPending_SetsMatchStatusFalse()
        {
            var relation = new Friend { Status = FriendStatus.Pending };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            await service.BlockUserAsync(blockerId, targetId);

            mockFriendRepo.Verify(r => r.SetMatchStatusAsync(blockerId, targetId, false), Times.Once);
        }

        [Fact]
        public async Task BlockUserAsync_WithPriorRelation_UpdatesStatusToBlocked()
        {
            var relation = new Friend { Status = FriendStatus.Accepted };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            await service.BlockUserAsync(blockerId, targetId);

            mockFriendRepo.Verify(r => r.UpdateFriendshipStatusAsync(blockerId, targetId, FriendStatus.Blocked), Times.Once);
        }

        // --- UnblockUserAsync Tests ---

        [Fact]
        public async Task UnblockUserAsync_NoPriorRelation_DoesNothing()
        {
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync((Friend?)null);

            await service.UnblockUserAsync(blockerId, targetId);

            mockFriendRepo.Verify(r => r.DeleteFriendshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UnblockUserAsync_RelationBlockedAndWasMatch_RestoresToAccepted()
        {
            var relation = new Friend { Status = FriendStatus.Blocked, IsMatch = true };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            await service.UnblockUserAsync(blockerId, targetId);

            mockFriendRepo.Verify(r => r.UpdateFriendshipStatusAsync(blockerId, targetId, FriendStatus.Accepted), Times.Once);
        }

        [Fact]
        public async Task UnblockUserAsync_RelationBlockedAndWasNotMatch_RestoresToPending()
        {
            var relation = new Friend { Status = FriendStatus.Blocked, IsMatch = false };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            await service.UnblockUserAsync(blockerId, targetId);

            mockFriendRepo.Verify(r => r.UpdateFriendshipStatusAsync(blockerId, targetId, FriendStatus.Pending), Times.Once);
        }

        [Fact]
        public async Task UnblockUserAsync_RelationNotBlocked_DeletesFriendship()
        {
            var relation = new Friend { Status = FriendStatus.Accepted };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            await service.UnblockUserAsync(blockerId, targetId);

            mockFriendRepo.Verify(r => r.DeleteFriendshipAsync(blockerId, targetId), Times.Once);
        }

        // --- GetBlockedUsersAsync Tests ---

        [Fact]
        public async Task GetBlockedUsersAsync_SkipsNonBlockedRelations()
        {
            var relations = new List<Friend> { new Friend { Status = FriendStatus.Accepted } };
            mockFriendRepo.Setup(r => r.GetAllFriendshipsForUserAsync(blockerId)).ReturnsAsync(relations);

            var result = await service.GetBlockedUsersAsync(blockerId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBlockedUsersAsync_WhenUserFound_AddsToList()
        {
            var relations = new List<Friend>
            {
                new Friend { Status = FriendStatus.Blocked, UserId1 = blockerId, UserId2 = targetId }
            };
            var mockUser = new User { Id = targetId, Username = "BlockedGuy" };

            mockFriendRepo.Setup(r => r.GetAllFriendshipsForUserAsync(blockerId)).ReturnsAsync(relations);
            mockUserRepo.Setup(r => r.GetByIdAsync(targetId)).ReturnsAsync(mockUser);

            var result = await service.GetBlockedUsersAsync(blockerId);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetBlockedUsersAsync_WhenUserNotFound_DoesNotAddToList()
        {
            var relations = new List<Friend>
            {
                new Friend { Status = FriendStatus.Blocked, UserId1 = blockerId, UserId2 = targetId }
            };

            mockFriendRepo.Setup(r => r.GetAllFriendshipsForUserAsync(blockerId)).ReturnsAsync(relations);
            mockUserRepo.Setup(r => r.GetByIdAsync(targetId)).ReturnsAsync((User?)null);

            var result = await service.GetBlockedUsersAsync(blockerId);

            Assert.Empty(result); // Tests the 'if (userObject != null)' false branch
        }

        // --- IsBlockedAsync & CheckIfBlockedAsync Tests ---

        [Fact]
        public async Task IsBlockedAsync_WhenBlocked_ReturnsTrue()
        {
            var relation = new Friend { Status = FriendStatus.Blocked };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            var result = await service.IsBlockedAsync(blockerId, targetId);

            Assert.True(result);
        }

        [Fact]
        public async Task IsBlockedAsync_WhenNotBlocked_ReturnsFalse()
        {
            var relation = new Friend { Status = FriendStatus.Accepted };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            var result = await service.IsBlockedAsync(blockerId, targetId);

            Assert.False(result);
        }

        [Fact]
        public async Task CheckIfBlockedAsync_WhenBlocked_ReturnsTrue()
        {
            var relation = new Friend { Status = FriendStatus.Blocked };
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(blockerId, targetId)).ReturnsAsync(relation);

            var result = await service.CheckIfBlockedAsync(blockerId, targetId);

            Assert.True(result);
        }
    }
}