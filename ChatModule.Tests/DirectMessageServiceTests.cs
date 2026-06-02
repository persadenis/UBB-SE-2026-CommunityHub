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
    public class DirectMessageServiceTests
    {
        private readonly Mock<IConversationRepository> _mockConvRepo;
        private readonly Mock<IParticipantRepository> _mockPartRepo;
        private readonly Mock<IFriendRepository> _mockFriendRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IMessageRepository> _mockMessageRepo;
        private readonly DirectMessageService _dmService;

        public DirectMessageServiceTests()
        {
            _mockConvRepo = new Mock<IConversationRepository>();
            _mockPartRepo = new Mock<IParticipantRepository>();
            _mockFriendRepo = new Mock<IFriendRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockMessageRepo = new Mock<IMessageRepository>();

            _dmService = new DirectMessageService(
                _mockConvRepo.Object,
                _mockPartRepo.Object,
                _mockFriendRepo.Object,
                _mockUserRepo.Object,
                _mockMessageRepo.Object
            );
        }

        [Fact]
        public async Task GetOrCreateAsync_DmExists_ReturnsExistingConversation()
        {
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();
            var existingDm = new Conversation { Id = Guid.NewGuid(), Type = ConversationType.Dm };

            _mockConvRepo.Setup(r => r.GetDmBetweenAsync(user1, user2)).ReturnsAsync(existingDm);

            var result = await _dmService.GetOrCreateAsync(user1, user2);

            Assert.Equal(existingDm.Id, result.Id);
            _mockConvRepo.Verify(r => r.CreateAsync(It.IsAny<Conversation>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateAsync_DmDoesNotExist_CreatesNew()
        {
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();
            _mockConvRepo.Setup(r => r.GetDmBetweenAsync(user1, user2)).ReturnsAsync((Conversation)null);

            var result = await _dmService.GetOrCreateAsync(user1, user2);

            Assert.NotNull(result);
            _mockConvRepo.Verify(r => r.CreateAsync(It.IsAny<Conversation>()), Times.Once);
            _mockPartRepo.Verify(r => r.CreateAsync(It.IsAny<Participant>()), Times.Exactly(2));
        }


        [Fact]
        public async Task PinMessageAsync_MessageNotFoundOrWrongConvo_ThrowsException()
        {
            var convoId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var msgId = Guid.NewGuid();
            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId))
                         .ReturnsAsync(new List<Participant> { new Participant { UserId = userId } });

            _mockMessageRepo.Setup(r => r.GetByIdAsync(msgId)).ReturnsAsync((Message)null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dmService.PinMessageAsync(convoId, userId, msgId, DateTime.Now));

            var wrongMsg = new Message { Id = msgId, ConversationId = Guid.NewGuid() };
            _mockMessageRepo.Setup(r => r.GetByIdAsync(msgId)).ReturnsAsync(wrongMsg);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _dmService.PinMessageAsync(convoId, userId, msgId, DateTime.Now));
        }

        [Fact]
        public async Task PinMessageAsync_ClearsOldPin_IfAlreadyExists()
        {
            var convoId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var oldMsgId = Guid.NewGuid();
            var newMsgId = Guid.NewGuid();

            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId)).ReturnsAsync(new List<Participant> { new Participant { UserId = userId } });
            _mockMessageRepo.Setup(r => r.GetByIdAsync(newMsgId)).ReturnsAsync(new Message { Id = newMsgId, ConversationId = convoId });

            var convo = new Conversation { Id = convoId, PinnedMessageId = oldMsgId };
            _mockConvRepo.Setup(r => r.GetByIdAsync(convoId)).ReturnsAsync(convo);

            await _dmService.PinMessageAsync(convoId, userId, newMsgId, DateTime.Now);

            _mockMessageRepo.Verify(r => r.SetPinExpiresAtAsync(oldMsgId, null), Times.Once);
        }

        [Fact]
        public async Task UnpinMessageAsync_ValidRequest_ClearsPin()
        {
            var convoId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var pinnedId = Guid.NewGuid();

            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId)).ReturnsAsync(new List<Participant> { new Participant { UserId = userId } });
            _mockConvRepo.Setup(r => r.GetByIdAsync(convoId)).ReturnsAsync(new Conversation { PinnedMessageId = pinnedId });

            await _dmService.UnpinMessageAsync(convoId, userId);

            _mockConvRepo.Verify(r => r.SetPinnedMessageAsync(convoId, null), Times.Once);
            _mockMessageRepo.Verify(r => r.SetPinExpiresAtAsync(pinnedId, null), Times.Once);
        }

        [Fact]
        public async Task ClearExpiredPinAsync_ExecutesRepos()
        {
            await _dmService.ClearExpiredPinAsync(Guid.NewGuid(), Guid.NewGuid());
            _mockMessageRepo.Verify(r => r.SetPinExpiresAtAsync(It.IsAny<Guid>(), null), Times.Once);
            _mockConvRepo.Verify(r => r.SetPinnedMessageAsync(It.IsAny<Guid>(), null), Times.Once);
        }

        [Fact]
        public async Task GetOtherUserAsync_ParticipantNotFound_ReturnsNull()
        {
            var convoId = Guid.NewGuid();
            var viewerId = Guid.NewGuid();
            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId))
                         .ReturnsAsync(new List<Participant> { new Participant { UserId = viewerId } });

            var result = await _dmService.GetOtherUserAsync(convoId, viewerId);
            Assert.Null(result);
        }

        [Fact]
        public async Task IsBlockedAsync_ViewerOrOtherBlocked_ReturnsTrue()
        {
            var convoId = Guid.NewGuid();
            var viewerId = Guid.NewGuid();
            var otherId = Guid.NewGuid();

            var participants = new List<Participant>
    {
        new Participant { UserId = viewerId },
        new Participant { UserId = otherId }
    };
            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId)).ReturnsAsync(participants);


            _mockFriendRepo.Setup(r => r.GetFriendshipAsync(otherId, viewerId))
                           .ReturnsAsync(new Friend { Status = FriendStatus.Blocked });

            _mockFriendRepo.Setup(r => r.GetFriendshipAsync(viewerId, otherId))
                           .ReturnsAsync((Friend)null);

            var result = await _dmService.IsBlockedAsync(convoId, viewerId);

            Assert.True(result);
        }

        [Fact]
        public async Task IsBlockedAsync_NoOtherParticipant_ReturnsFalse()
        {
            var convoId = Guid.NewGuid();
            var viewerId = Guid.NewGuid();
            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId))
                         .ReturnsAsync(new List<Participant> { new Participant { UserId = viewerId } });

            var result = await _dmService.IsBlockedAsync(convoId, viewerId);
            Assert.False(result); 
        }
        [Fact]
        public async Task GetOtherUserAsync_Success_ReturnsUser()
        {
            var convoId = Guid.NewGuid();
            var viewerId = Guid.NewGuid();
            var otherId = Guid.NewGuid();
            var otherUser = new User { Id = otherId, Username = "Target" };

            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId))
                         .ReturnsAsync(new List<Participant> {
                     new Participant { UserId = viewerId },
                     new Participant { UserId = otherId }
                         });

            _mockUserRepo.Setup(r => r.GetByIdAsync(otherId)).ReturnsAsync(otherUser);

            var result = await _dmService.GetOtherUserAsync(convoId, viewerId);

            Assert.NotNull(result);
            Assert.Equal("Target", result.Username); 
        }
        [Fact]
        public async Task PinMessageAsync_RequesterNotParticipant_ThrowsException()
        {
            var convoId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId))
                         .ReturnsAsync(new List<Participant> { new Participant { UserId = Guid.NewGuid() } });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _dmService.PinMessageAsync(convoId, strangerId, Guid.NewGuid(), DateTime.Now));
        }
        [Fact]
        public async Task UnpinMessageAsync_RequesterNotParticipant_ThrowsException()
        {
            var convoId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId))
                         .ReturnsAsync(new List<Participant> { new Participant { UserId = Guid.NewGuid() } });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _dmService.UnpinMessageAsync(convoId, strangerId));
        }
    }
}