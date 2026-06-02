using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;
using Moq;
using Xunit;

namespace ChatModule.Tests
{
    public class GroupServiceTests
    {
        private readonly Mock<IConversationRepository> _conversationRepositoryMock = new();
        private readonly Mock<IParticipantRepository> _participantRepositoryMock = new();
        private readonly Mock<IMessageRepository> _messageRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();

        private GroupService CreateService()
            => new(_conversationRepositoryMock.Object, _participantRepositoryMock.Object, _messageRepositoryMock.Object, _userRepositoryMock.Object);

        [Fact]
        public void Constructor_NullConversationRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupService(null!, _participantRepositoryMock.Object, _messageRepositoryMock.Object, _userRepositoryMock.Object));
        }

        [Fact]
        public void Constructor_NullParticipantRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupService(_conversationRepositoryMock.Object, null!, _messageRepositoryMock.Object, _userRepositoryMock.Object));
        }

        [Fact]
        public void Constructor_NullMessageRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupService(_conversationRepositoryMock.Object, _participantRepositoryMock.Object, null!, _userRepositoryMock.Object));
        }

        [Fact]
        public void Constructor_NullUserRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupService(_conversationRepositoryMock.Object, _participantRepositoryMock.Object, _messageRepositoryMock.Object, null!));
        }

        [Fact]
        public async Task CreateGroupAsync_WhitespaceTitle_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateGroupAsync(Guid.NewGuid(), "  ", null, new List<Guid>()));
        }

        [Fact]
        public async Task CreateGroupAsync_SanitizesAndDeduplicatesMembers()
        {
            var creatorId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var createdParticipants = new List<Participant>();

            _participantRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Participant>()))
                .Callback<Participant>(participant => createdParticipants.Add(participant))
                .Returns(Task.CompletedTask);

            _messageRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Message>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock
                .Setup(repository => repository.GetByIdAsync(creatorId))
                .ReturnsAsync(new User { Id = creatorId, Username = "creator" });

            var result = await CreateService().CreateGroupAsync(
                creatorId,
                "  Team  ",
                "icon.png",
                new List<Guid> { Guid.Empty, creatorId, memberId, memberId });

            Assert.True(result.Title == "Team" && createdParticipants.Count == 2 && createdParticipants.Single(participant => participant.UserId == creatorId).Role == ParticipantRole.Admin);
        }

        [Fact]
        public async Task CreateGroupAsync_MissingCreatorUser_UsesGuidInSystemMessage()
        {
            var creatorId = Guid.NewGuid();
            string? systemMessageText = null;

            _participantRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Participant>()))
                .Returns(Task.CompletedTask);

            _messageRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Message>()))
                .Callback<Message>(message => systemMessageText = message.Content)
                .Returns(Task.CompletedTask);

            var result = await CreateService().CreateGroupAsync(creatorId, "Team", null, new List<Guid>());

            Assert.Contains(creatorId.ToString(), systemMessageText);
        }

        [Fact]
        public async Task UpdateGroupInfoAsync_RequesterNotMember_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, requesterId))
                .ReturnsAsync((Participant?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().UpdateGroupInfoAsync(conversationId, requesterId, "New", "icon"));
        }

        [Fact]
        public async Task UpdateGroupInfoAsync_RequesterNotAdmin_ThrowsUnauthorizedAccessException()
        {
            var conversationId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, requesterId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Member });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CreateService().UpdateGroupInfoAsync(conversationId, requesterId, "New", "icon"));
        }

        [Fact]
        public async Task UpdateGroupInfoAsync_ConversationMissing_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, requesterId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _conversationRepositoryMock
                .Setup(repository => repository.GetByIdAsync(conversationId))
                .ReturnsAsync((Conversation?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().UpdateGroupInfoAsync(conversationId, requesterId, "New", "icon"));
        }

        [Fact]
        public async Task UpdateGroupInfoAsync_UpdatesProvidedFields()
        {
            var conversationId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            Conversation? updatedConversation = null;
            var conversation = new Conversation { Id = conversationId, Title = "Old", IconUrl = "old.png" };

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, requesterId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _conversationRepositoryMock
                .Setup(repository => repository.GetByIdAsync(conversationId))
                .ReturnsAsync(conversation);
            _conversationRepositoryMock
                .Setup(repository => repository.UpdateAsync(It.IsAny<Conversation>()))
                .Callback<Conversation>(value => updatedConversation = value)
                .Returns(Task.CompletedTask);

            await CreateService().UpdateGroupInfoAsync(conversationId, requesterId, "New", "new.png");

            Assert.True(updatedConversation?.Title == "New" && updatedConversation.IconUrl == "new.png");
        }

        [Fact]
        public async Task LeaveGroupAsync_NonMember_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, userId))
                .ReturnsAsync((Participant?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().LeaveGroupAsync(conversationId, userId));
        }

        [Fact]
        public async Task LeaveGroupAsync_LastParticipant_DeletesConversationAndMessages()
        {
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var deletedConversation = false;
            var deletedMessages = false;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, userId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Member });
            _participantRepositoryMock
                .Setup(repository => repository.DeleteAsync(conversationId, userId))
                .Returns(Task.CompletedTask);
            _participantRepositoryMock
                .Setup(repository => repository.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant>());
            _messageRepositoryMock
                .Setup(repository => repository.DeleteByConversationAsync(conversationId))
                .Callback(() => deletedMessages = true)
                .Returns(Task.CompletedTask);
            _conversationRepositoryMock
                .Setup(repository => repository.DeleteAsync(conversationId))
                .Callback(() => deletedConversation = true)
                .Returns(Task.CompletedTask);

            await CreateService().LeaveGroupAsync(conversationId, userId);

            Assert.True(deletedConversation && deletedMessages);
        }

        [Fact]
        public async Task LeaveGroupAsync_LastAdmin_PromotesOldestRemainingParticipant()
        {
            var conversationId = Guid.NewGuid();
            var leavingAdminId = Guid.NewGuid();
            var oldestUserId = Guid.NewGuid();
            var newerUserId = Guid.NewGuid();
            var promotedUserId = Guid.Empty;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, leavingAdminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin, UserId = leavingAdminId });
            _participantRepositoryMock
                .Setup(repository => repository.DeleteAsync(conversationId, leavingAdminId))
                .Returns(Task.CompletedTask);
            _participantRepositoryMock
                .Setup(repository => repository.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { UserId = newerUserId, Role = ParticipantRole.Member, JoinedAt = new DateTime(2024, 1, 2) },
                    new() { UserId = oldestUserId, Role = ParticipantRole.Member, JoinedAt = new DateTime(2024, 1, 1) },
                });
            _participantRepositoryMock
                .Setup(repository => repository.UpdateRoleAsync(conversationId, It.IsAny<Guid>(), ParticipantRole.Admin))
                .Callback<Guid, Guid, ParticipantRole>((_, userId, _) => promotedUserId = userId)
                .Returns(Task.CompletedTask);
            _userRepositoryMock
                .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => new User { Id = id, Username = id.ToString() });
            _messageRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Message>()))
                .Returns(Task.CompletedTask);

            await CreateService().LeaveGroupAsync(conversationId, leavingAdminId);

            Assert.Equal(oldestUserId, promotedUserId);
        }

        [Fact]
        public async Task LeaveGroupAsync_WhenAdminRemains_DoesNotPromoteAnyone()
        {
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var roleUpdated = false;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, userId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Member, UserId = userId });
            _participantRepositoryMock
                .Setup(repository => repository.DeleteAsync(conversationId, userId))
                .Returns(Task.CompletedTask);
            _participantRepositoryMock
                .Setup(repository => repository.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant> { new() { UserId = Guid.NewGuid(), Role = ParticipantRole.Admin } });
            _participantRepositoryMock
                .Setup(repository => repository.UpdateRoleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ParticipantRole>()))
                .Callback(() => roleUpdated = true)
                .Returns(Task.CompletedTask);
            _userRepositoryMock
                .Setup(repository => repository.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Username = "user" });
            _messageRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Message>()))
                .Returns(Task.CompletedTask);

            await CreateService().LeaveGroupAsync(conversationId, userId);

            Assert.False(roleUpdated);
        }

        [Fact]
        public async Task PinMessageAsync_MessageMissing_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, requesterId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _messageRepositoryMock
                .Setup(repository => repository.GetByIdAsync(messageId))
                .ReturnsAsync((Message?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().PinMessageAsync(conversationId, requesterId, messageId));
        }

        [Fact]
        public async Task PinMessageAsync_MessageFromDifferentConversation_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, requesterId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _messageRepositoryMock
                .Setup(repository => repository.GetByIdAsync(messageId))
                .ReturnsAsync(new Message { Id = messageId, ConversationId = Guid.NewGuid() });

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().PinMessageAsync(conversationId, requesterId, messageId));
        }

        [Fact]
        public async Task PinMessageAsync_ValidMessage_PinsMessage()
        {
            var conversationId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            Guid? pinnedMessageId = Guid.Empty;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, requesterId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _messageRepositoryMock
                .Setup(repository => repository.GetByIdAsync(messageId))
                .ReturnsAsync(new Message { Id = messageId, ConversationId = conversationId });
            _conversationRepositoryMock
                .Setup(repository => repository.SetPinnedMessageAsync(conversationId, messageId))
                .Callback<Guid, Guid?>((_, value) => pinnedMessageId = value)
                .Returns(Task.CompletedTask);

            await CreateService().PinMessageAsync(conversationId, requesterId, messageId);

            Assert.Equal(messageId, pinnedMessageId);
        }

        [Fact]
        public async Task UnpinMessageAsync_ClearsPinnedMessage()
        {
            var conversationId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            Guid? pinnedMessageId = Guid.NewGuid();

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, requesterId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _conversationRepositoryMock
                .Setup(repository => repository.SetPinnedMessageAsync(conversationId, null))
                .Callback<Guid, Guid?>((_, value) => pinnedMessageId = value)
                .Returns(Task.CompletedTask);

            await CreateService().UnpinMessageAsync(conversationId, requesterId);

            Assert.Null(pinnedMessageId);
        }

        [Fact]
        public async Task PostEventNoticeAsync_WritesSystemMessage()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var eventDate = new DateTime(2026, 4, 17, 10, 30, 0);
            string? text = null;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _messageRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Message>()))
                .Callback<Message>(message => text = message.Content)
                .Returns(Task.CompletedTask);

            await CreateService().PostEventNoticeAsync(conversationId, adminId, "Demo", eventDate);

            Assert.Contains("Event: \"Demo\"", text);
        }
    }
}
