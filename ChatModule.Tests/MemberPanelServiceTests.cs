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
    public class MemberPanelServiceTests
    {
        private readonly Mock<IParticipantRepository> _participantRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();

        private MemberPanelService CreateService()
            => new(_participantRepositoryMock.Object, _userRepositoryMock.Object);

        [Fact]
        public void Constructor_NullParticipantRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberPanelService(null!, _userRepositoryMock.Object));
        }

        [Fact]
        public void Constructor_NullUserRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberPanelService(_participantRepositoryMock.Object, null!));
        }

        [Fact]
        public async Task GetMembersAsync_ReturnsRepositoryMembers()
        {
            var conversationId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant> { new(), new() });

            var result = await CreateService().GetMembersAsync(conversationId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetBannedMembersAsync_ReturnsOnlyBannedParticipants()
        {
            var conversationId = Guid.NewGuid();
            _participantRepositoryMock
                .Setup(repository => repository.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant>
                {
                    new() { Role = ParticipantRole.Member },
                    new() { Role = ParticipantRole.Banned },
                    new() { Role = ParticipantRole.Admin },
                });

            var result = await CreateService().GetBannedMembersAsync(conversationId);

            Assert.Single(result);
        }

        [Fact]
        public async Task SearchUsersToAddAsync_Whitespace_ReturnsEmptyList()
        {
            var result = await CreateService().SearchUsersToAddAsync(Guid.NewGuid(), "   ");

            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchUsersToAddAsync_ExcludesExistingParticipants()
        {
            var conversationId = Guid.NewGuid();
            var existingUserId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();

            _participantRepositoryMock
                .Setup(repository => repository.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant> { new() { UserId = existingUserId } });

            _userRepositoryMock
                .Setup(repository => repository.SearchByUsernameAsync("ann"))
                .ReturnsAsync(new List<User>
                {
                    new() { Id = existingUserId, Username = "existing" },
                    new() { Id = newUserId, Username = "new" },
                });

            var result = await CreateService().SearchUsersToAddAsync(conversationId, "ann");

            Assert.Equal(newUserId, result.Single().Id);
        }

        [Fact]
        public async Task GetUserAsync_ReturnsRepositoryUser()
        {
            var userId = Guid.NewGuid();
            _userRepositoryMock
                .Setup(repository => repository.GetByIdAsync(userId))
                .ReturnsAsync(new User { Id = userId, Username = "anna" });

            var result = await CreateService().GetUserAsync(userId);

            Assert.Equal(userId, result?.Id);
        }
    }
}
