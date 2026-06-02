using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;
using Moq;
using Xunit;

namespace ChatModule.Tests
{
    public class ModerationServiceTests
    {
        private readonly Mock<IParticipantRepository> _participantRepositoryMock = new();
        private readonly Mock<IMessageRepository> _messageRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();

        private ModerationService CreateService()
            => new(_participantRepositoryMock.Object, _messageRepositoryMock.Object, _userRepositoryMock.Object);

        [Fact]
        public void Constructor_NullParticipantRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ModerationService(null!, _messageRepositoryMock.Object, _userRepositoryMock.Object));
        }

        [Fact]
        public void Constructor_NullMessageRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ModerationService(_participantRepositoryMock.Object, null!, _userRepositoryMock.Object));
        }

        [Fact]
        public void Constructor_NullUserRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ModerationService(_participantRepositoryMock.Object, _messageRepositoryMock.Object, null!));
        }

        [Fact]
        public async Task BanMemberAsync_RequesterNotAdmin_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync((Participant?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().BanMemberAsync(conversationId, adminId, targetId));
        }

        [Fact]
        public async Task BanMemberAsync_TargetMissing_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, targetId))
                .ReturnsAsync((Participant?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().BanMemberAsync(conversationId, adminId, targetId));
        }

        [Fact]
        public async Task BanMemberAsync_ValidInput_UpdatesRoleToBanned()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var updatedRole = ParticipantRole.Admin;

            SetupAdminAndTarget(conversationId, adminId, targetId);
            _participantRepositoryMock
                .Setup(repository => repository.UpdateRoleAsync(conversationId, targetId, ParticipantRole.Banned))
                .Callback<Guid, Guid, ParticipantRole>((_, _, role) => updatedRole = role)
                .Returns(Task.CompletedTask);
            _messageRepositoryMock.Setup(repository => repository.CreateAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(repository => repository.GetByIdAsync(targetId)).ReturnsAsync(new User { Id = targetId, Username = "user" });

            await CreateService().BanMemberAsync(conversationId, adminId, targetId);

            Assert.Equal(ParticipantRole.Banned, updatedRole);
        }

        [Fact]
        public async Task UnbanMemberAsync_ValidInput_UpdatesRoleToMember()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var updatedRole = ParticipantRole.Admin;

            SetupAdminAndTarget(conversationId, adminId, targetId);
            _participantRepositoryMock
                .Setup(repository => repository.UpdateRoleAsync(conversationId, targetId, ParticipantRole.Member))
                .Callback<Guid, Guid, ParticipantRole>((_, _, role) => updatedRole = role)
                .Returns(Task.CompletedTask);
            _messageRepositoryMock.Setup(repository => repository.CreateAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(repository => repository.GetByIdAsync(targetId)).ReturnsAsync(new User { Id = targetId, Username = "user" });

            await CreateService().UnbanMemberAsync(conversationId, adminId, targetId);

            Assert.Equal(ParticipantRole.Member, updatedRole);
        }

        [Fact]
        public async Task TimeoutMemberAsync_NonPositiveDuration_ThrowsArgumentException()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            SetupAdminAndTarget(conversationId, adminId, targetId);

            await Assert.ThrowsAsync<ArgumentException>(() => CreateService().TimeoutMemberAsync(conversationId, adminId, targetId, TimeSpan.Zero));
        }

        [Theory]
        [InlineData(0, 0, 2, "2 days")]
        [InlineData(1, 30, 0, "1 hour 30 minutes")]
        [InlineData(0, 1, 0, "1 minute")]
        public async Task TimeoutMemberAsync_FormatsDurationInSystemMessage(int hours, int minutes, int days, string expectedLabel)
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var duration = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            string? text = null;

            SetupAdminAndTarget(conversationId, adminId, targetId);
            _participantRepositoryMock
                .Setup(repository => repository.UpdateTimeoutAsync(conversationId, targetId, It.IsAny<DateTime?>()))
                .Returns(Task.CompletedTask);
            _messageRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Message>()))
                .Callback<Message>(message => text = message.Content)
                .Returns(Task.CompletedTask);
            _userRepositoryMock
                .Setup(repository => repository.GetByIdAsync(targetId))
                .ReturnsAsync(new User { Id = targetId, Username = "user" });

            await CreateService().TimeoutMemberAsync(conversationId, adminId, targetId, duration);

            Assert.Contains(expectedLabel, text);
        }

        [Fact]
        public async Task RemoveTimeoutAsync_UsesNullTimeout()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            DateTime? timeoutValue = DateTime.UtcNow;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _participantRepositoryMock
                .Setup(repository => repository.UpdateTimeoutAsync(conversationId, targetId, null))
                .Callback<Guid, Guid, DateTime?>((_, _, value) => timeoutValue = value)
                .Returns(Task.CompletedTask);
            _messageRepositoryMock.Setup(repository => repository.CreateAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(repository => repository.GetByIdAsync(targetId)).ReturnsAsync((User?)null);

            await CreateService().RemoveTimeoutAsync(conversationId, adminId, targetId);

            Assert.Null(timeoutValue);
        }

        [Fact]
        public async Task PromoteMemberAsync_UpdatesRoleToAdmin()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            var updatedRole = ParticipantRole.Member;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _participantRepositoryMock
                .Setup(repository => repository.UpdateRoleAsync(conversationId, targetId, ParticipantRole.Admin))
                .Callback<Guid, Guid, ParticipantRole>((_, _, role) => updatedRole = role)
                .Returns(Task.CompletedTask);
            _messageRepositoryMock.Setup(repository => repository.CreateAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(repository => repository.GetByIdAsync(targetId)).ReturnsAsync(new User { Id = targetId, Username = "user" });

            await CreateService().PromoteMemberAsync(conversationId, adminId, targetId);

            Assert.Equal(ParticipantRole.Admin, updatedRole);
        }

        [Fact]
        public async Task DemoteMemberAsync_OnlyAdminDemotingSelf_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _participantRepositoryMock
                .Setup(repository => repository.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant> { new() { UserId = adminId, Role = ParticipantRole.Admin } });

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().DemoteMemberAsync(conversationId, adminId, adminId));
        }

        [Fact]
        public async Task DemoteMemberAsync_WithAnotherAdmin_UpdatesRoleToMember()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var updatedRole = ParticipantRole.Admin;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _participantRepositoryMock
                .Setup(repository => repository.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { UserId = adminId, Role = ParticipantRole.Admin },
                    new() { UserId = Guid.NewGuid(), Role = ParticipantRole.Admin },
                });
            _participantRepositoryMock
                .Setup(repository => repository.UpdateRoleAsync(conversationId, adminId, ParticipantRole.Member))
                .Callback<Guid, Guid, ParticipantRole>((_, _, role) => updatedRole = role)
                .Returns(Task.CompletedTask);
            _messageRepositoryMock.Setup(repository => repository.CreateAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(repository => repository.GetByIdAsync(adminId)).ReturnsAsync((User?)null);

            await CreateService().DemoteMemberAsync(conversationId, adminId, adminId);

            Assert.Equal(ParticipantRole.Member, updatedRole);
        }

        [Fact]
        public async Task AddMemberAsync_UserMissing_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _userRepositoryMock
                .Setup(repository => repository.GetByIdAsync(newUserId))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().AddMemberAsync(conversationId, adminId, newUserId));
        }

        [Fact]
        public async Task AddMemberAsync_ExistingParticipant_ThrowsInvalidOperationException()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _userRepositoryMock
                .Setup(repository => repository.GetByIdAsync(newUserId))
                .ReturnsAsync(new User { Id = newUserId, Username = "user" });
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, newUserId))
                .ReturnsAsync(new Participant { UserId = newUserId, Role = ParticipantRole.Member });

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().AddMemberAsync(conversationId, adminId, newUserId));
        }

        [Fact]
        public async Task AddMemberAsync_ValidInput_CreatesMemberParticipant()
        {
            var conversationId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();
            Participant? createdParticipant = null;

            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin });
            _userRepositoryMock
                .Setup(repository => repository.GetByIdAsync(newUserId))
                .ReturnsAsync(new User { Id = newUserId, Username = "user" });
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, newUserId))
                .ReturnsAsync((Participant?)null);
            _participantRepositoryMock
                .Setup(repository => repository.CreateAsync(It.IsAny<Participant>()))
                .Callback<Participant>(participant => createdParticipant = participant)
                .Returns(Task.CompletedTask);
            _messageRepositoryMock.Setup(repository => repository.CreateAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

            await CreateService().AddMemberAsync(conversationId, adminId, newUserId);

            Assert.True(createdParticipant?.UserId == newUserId && createdParticipant.Role == ParticipantRole.Member);
        }

        private void SetupAdminAndTarget(Guid conversationId, Guid adminId, Guid targetId)
        {
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, adminId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Admin, UserId = adminId });
            _participantRepositoryMock
                .Setup(repository => repository.GetAsync(conversationId, targetId))
                .ReturnsAsync(new Participant { Role = ParticipantRole.Member, UserId = targetId });
        }
    }
}
