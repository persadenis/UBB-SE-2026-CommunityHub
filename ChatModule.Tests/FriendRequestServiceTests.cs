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
    public class FriendRequestServiceTests
    {
        private readonly Mock<IFriendRepository> mockFriendRepo;
        private readonly Mock<IUserRepository> mockUserRepo;
        private readonly Mock<IConversationRepository> mockConvRepo;
        private readonly Mock<IParticipantRepository> mockPartRepo;
        private readonly FriendRequestService service;

        public FriendRequestServiceTests()
        {
            mockFriendRepo = new Mock<IFriendRepository>();
            mockUserRepo = new Mock<IUserRepository>();

            mockConvRepo = new Mock<IConversationRepository>();
            mockPartRepo = new Mock<IParticipantRepository>();

            service = new FriendRequestService(
                mockFriendRepo.Object,
                mockUserRepo.Object,
                mockConvRepo.Object,
                mockPartRepo.Object);
        }

        // SendFriendRequestAsync 

        [Fact]
        public async Task SendFriendRequestAsync_ToSelf_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var exception = await Record.ExceptionAsync(() => service.SendFriendRequestAsync(userId, userId));
            Assert.IsType<InvalidOperationException>(exception);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ToSelf_ExceptionHasCorrectMessage()
        {
            var userId = Guid.NewGuid();
            var exception = await Record.ExceptionAsync(() => service.SendFriendRequestAsync(userId, userId));
            Assert.Equal("You cannot send a friend request to yourself.", exception.Message);
        }

        [Fact]
        public async Task SendFriendRequestAsync_AlreadyFriends_ThrowsException()
        {
            mockFriendRepo.Setup(r => r.CheckIfFriendsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

            var exception = await Record.ExceptionAsync(() => service.SendFriendRequestAsync(Guid.NewGuid(), Guid.NewGuid()));
            Assert.Equal("Users are already friends.", exception.Message);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ExistingRelationBlocked_UpdatesStatusToPending()
        {
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            mockFriendRepo.Setup(r => r.CheckIfFriendsAsync(senderId, receiverId)).ReturnsAsync(false);
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(senderId, receiverId))
                .ReturnsAsync(new Friend { Status = FriendStatus.Blocked });

            await service.SendFriendRequestAsync(senderId, receiverId);

            mockFriendRepo.Verify(r => r.UpdateFriendshipStatusAsync(senderId, receiverId, FriendStatus.Pending), Times.Once);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ExistingRelationBlocked_SetsMatchStatusToFalse()
        {
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            mockFriendRepo.Setup(r => r.CheckIfFriendsAsync(senderId, receiverId)).ReturnsAsync(false);
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(senderId, receiverId))
                .ReturnsAsync(new Friend { Status = FriendStatus.Blocked });

            await service.SendFriendRequestAsync(senderId, receiverId);

            mockFriendRepo.Verify(r => r.SetMatchStatusAsync(senderId, receiverId, false), Times.Once);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ExistingRelationNotBlocked_ThrowsException()
        {
            mockFriendRepo.Setup(r => r.CheckIfFriendsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Friend { Status = FriendStatus.Pending });

            var exception = await Record.ExceptionAsync(() => service.SendFriendRequestAsync(Guid.NewGuid(), Guid.NewGuid()));
            Assert.Equal("A friend request already exists between these users.", exception.Message);
        }

        [Fact]
        public async Task SendFriendRequestAsync_NoExistingRelation_CreatesNewRequest()
        {
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            mockFriendRepo.Setup(r => r.CheckIfFriendsAsync(senderId, receiverId)).ReturnsAsync(false);
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(senderId, receiverId)).ReturnsAsync((Friend?)null);

            await service.SendFriendRequestAsync(senderId, receiverId);

            mockFriendRepo.Verify(r => r.CreateFriendshipAsync(It.Is<Friend>(f => f.Status == FriendStatus.Pending)), Times.Once);
        }

        // SendFriendRequestByUsernameAsync 

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task SendFriendRequestByUsernameAsync_EmptyUsername_ThrowsException(string? invalidUsername)
        {
            var exception = await Record.ExceptionAsync(() => service.SendFriendRequestByUsernameAsync(Guid.NewGuid(), invalidUsername!));
            Assert.Equal("Please enter a username.", exception.Message);
        }

        [Fact]
        public async Task SendFriendRequestByUsernameAsync_UserNotFound_ReturnsFalse()
        {
            mockUserRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var result = await service.SendFriendRequestByUsernameAsync(Guid.NewGuid(), "Ghost");
            Assert.False(result);
        }

        [Fact]
        public async Task SendFriendRequestByUsernameAsync_UserFound_ReturnsTrue()
        {
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            mockUserRepo.Setup(r => r.GetByUsernameAsync("Target")).ReturnsAsync(new User { Id = receiverId });
            mockFriendRepo.Setup(r => r.CheckIfFriendsAsync(senderId, receiverId)).ReturnsAsync(false);
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(senderId, receiverId)).ReturnsAsync((Friend?)null);

            var result = await service.SendFriendRequestByUsernameAsync(senderId, "Target");

            Assert.True(result);
        }

        [Fact]
        public async Task SendFriendRequestByUsernameAsync_UserFound_CreatesFriendship()
        {
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            mockUserRepo.Setup(r => r.GetByUsernameAsync("Target")).ReturnsAsync(new User { Id = receiverId });
            mockFriendRepo.Setup(r => r.CheckIfFriendsAsync(senderId, receiverId)).ReturnsAsync(false);
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(senderId, receiverId)).ReturnsAsync((Friend?)null);

            await service.SendFriendRequestByUsernameAsync(senderId, "Target");

            mockFriendRepo.Verify(r => r.CreateFriendshipAsync(It.IsAny<Friend>()), Times.Once);
        }

        // AcceptFriendRequestAsync 

        [Fact]
        public async Task AcceptFriendRequestAsync_ConversationExists_UpdatesFriendshipStatus()
        {
            var currentId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            mockConvRepo.Setup(r => r.GetDmBetweenAsync(currentId, requesterId)).ReturnsAsync(new Conversation());

            await service.AcceptFriendRequestAsync(currentId, requesterId);

            mockFriendRepo.Verify(r => r.UpdateFriendshipStatusAsync(requesterId, currentId, FriendStatus.Accepted), Times.Once);
        }

        [Fact]
        public async Task AcceptFriendRequestAsync_ConversationExists_DoesNotCreateConversation()
        {
            var currentId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            mockConvRepo.Setup(r => r.GetDmBetweenAsync(currentId, requesterId)).ReturnsAsync(new Conversation());

            await service.AcceptFriendRequestAsync(currentId, requesterId);

            mockConvRepo.Verify(r => r.CreateAsync(It.IsAny<Conversation>()), Times.Never);
        }

        [Fact]
        public async Task AcceptFriendRequestAsync_NoConversation_CreatesConversation()
        {
            var currentId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            mockConvRepo.Setup(r => r.GetDmBetweenAsync(currentId, requesterId)).ReturnsAsync((Conversation?)null);

            await service.AcceptFriendRequestAsync(currentId, requesterId);

            mockConvRepo.Verify(r => r.CreateAsync(It.IsAny<Conversation>()), Times.Once);
        }

        [Fact]
        public async Task AcceptFriendRequestAsync_NoConversation_CreatesTwoParticipants()
        {
            var currentId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            mockConvRepo.Setup(r => r.GetDmBetweenAsync(currentId, requesterId)).ReturnsAsync((Conversation?)null);

            await service.AcceptFriendRequestAsync(currentId, requesterId);

            mockPartRepo.Verify(r => r.CreateAsync(It.IsAny<Participant>()), Times.Exactly(2));
        }

        // DeclineFriendRequestAsync

        [Fact]
        public async Task DeclineFriendRequestAsync_RelationNull_ThrowsException()
        {
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Friend?)null);

            var exception = await Record.ExceptionAsync(() => service.DeclineFriendRequestAsync(Guid.NewGuid(), Guid.NewGuid()));
            Assert.Equal("No pending friend request found.", exception.Message);
        }

        [Fact]
        public async Task DeclineFriendRequestAsync_StatusNotPending_ThrowsException()
        {
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Friend { Status = FriendStatus.Accepted });

            var exception = await Record.ExceptionAsync(() => service.DeclineFriendRequestAsync(Guid.NewGuid(), Guid.NewGuid()));
            Assert.Equal("No pending friend request found.", exception.Message);
        }

        [Fact]
        public async Task DeclineFriendRequestAsync_ValidRequest_DeletesFriendship()
        {
            var currentId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(requesterId, currentId))
                .ReturnsAsync(new Friend { Status = FriendStatus.Pending });

            await service.DeclineFriendRequestAsync(currentId, requesterId);

            mockFriendRepo.Verify(r => r.DeleteFriendshipAsync(requesterId, currentId), Times.Once);
        }

        // GetIncomingRequestsAsync

        [Fact]
        public async Task GetIncomingRequestsAsync_EvaluatesBothBranches_ReturnsSingleItem()
        {
            var currentId = Guid.NewGuid();
            var validSenderId = Guid.NewGuid();
            var ghostSenderId = Guid.NewGuid();

            var pendingList = new List<Friend>
            {
                new Friend { UserId1 = validSenderId },
                new Friend { UserId1 = ghostSenderId }
            };

            mockFriendRepo.Setup(r => r.GetPendingRequestsForUserAsync(currentId)).ReturnsAsync(pendingList);
            mockUserRepo.Setup(r => r.GetByIdAsync(validSenderId)).ReturnsAsync(new User { Id = validSenderId });
            mockUserRepo.Setup(r => r.GetByIdAsync(ghostSenderId)).ReturnsAsync((User?)null);

            var result = await service.GetIncomingRequestsAsync(currentId);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetIncomingRequestsAsync_EvaluatesBothBranches_ReturnsCorrectUser()
        {
            var currentId = Guid.NewGuid();
            var validSenderId = Guid.NewGuid();
            var ghostSenderId = Guid.NewGuid();

            var pendingList = new List<Friend>
            {
                new Friend { UserId1 = validSenderId },
                new Friend { UserId1 = ghostSenderId }
            };

            mockFriendRepo.Setup(r => r.GetPendingRequestsForUserAsync(currentId)).ReturnsAsync(pendingList);
            mockUserRepo.Setup(r => r.GetByIdAsync(validSenderId)).ReturnsAsync(new User { Id = validSenderId });
            mockUserRepo.Setup(r => r.GetByIdAsync(ghostSenderId)).ReturnsAsync((User?)null);

            var result = await service.GetIncomingRequestsAsync(currentId);

            Assert.Equal(validSenderId, result[0].Id);
        }

        // GetRelationshipStatusAsync

        [Fact]
        public async Task GetRelationshipStatusAsync_NullRelation_ReturnsNull()
        {
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Friend?)null);

            var result = await service.GetRelationshipStatusAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetRelationshipStatusAsync_ValidRelation_ReturnsStatus()
        {
            mockFriendRepo.Setup(r => r.GetFriendshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Friend { Status = FriendStatus.Blocked });

            var result = await service.GetRelationshipStatusAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Equal(FriendStatus.Blocked, result);
        }
    }
}