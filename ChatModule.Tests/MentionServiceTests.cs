using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ChatModule.Tests.Services
{
    public class MentionServiceTests
    {
        private readonly Mock<IParticipantRepository> _mockParticipantRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly MentionService _mentionService;

        public MentionServiceTests()
        {
            this._mockParticipantRepository = new Mock<IParticipantRepository>();
            this._mockUserRepository = new Mock<IUserRepository>();

            this._mentionService = new MentionService(
                this._mockParticipantRepository.Object,
                this._mockUserRepository.Object);
        }

        [Fact]
        public async Task GetCandidatesAsync_ReturnsOnlyMatchingUsersInConversation()
        {
            var conversationId = Guid.NewGuid();
            var participantId = Guid.NewGuid();
            var externalUserId = Guid.NewGuid(); // Un user care se potrivește căutării, dar nu e în grup

            this._mockParticipantRepository
                .Setup(repo => repo.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant> { new Participant { UserId = participantId } });

            var matchingUsers = new List<User>
            {
                new User { Id = participantId, Username = "test_user_inside" },
                new User { Id = externalUserId, Username = "test_user_outside" }
            };

            this._mockUserRepository
                .Setup(repo => repo.SearchByUsernameAsync("test"))
                .ReturnsAsync(matchingUsers);

            var result = await this._mentionService.GetCandidatesAsync(conversationId, "test");

            // Singurul Assert verifică dacă lista filtrată are exact 1 element (cel din grup)
            Assert.Single(result);
        }

        [Fact]
        public async Task ExtractMentionedUserIdsAsync_EmptyContent_ReturnsEmptyList()
        {
            var conversationId = Guid.NewGuid();

            var result = await this._mentionService.ExtractMentionedUserIdsAsync(conversationId, "   ");

            Assert.Empty(result);
        }

        [Fact]
        public async Task ExtractMentionedUserIdsAsync_ValidMentions_ReturnsParticipantUserIds()
        {
            var conversationId = Guid.NewGuid();
            var participantId = Guid.NewGuid();
            var content = "Hello @JohnDoe, how are you?";

            this._mockParticipantRepository
                .Setup(repo => repo.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant> { new Participant { UserId = participantId } });

            this._mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync("JohnDoe"))
                .ReturnsAsync(new User { Id = participantId, Username = "JohnDoe" });

            var result = await this._mentionService.ExtractMentionedUserIdsAsync(conversationId, content);

            Assert.Contains(participantId, result);
        }

        [Fact]
        public async Task ExtractMentionedUserIdsAsync_MentionsNonParticipants_IgnoresThem()
        {
            var conversationId = Guid.NewGuid();
            var content = "Hello @GhostUser";

            this._mockParticipantRepository
                .Setup(repo => repo.GetAllForConversationAsync(conversationId))
                .ReturnsAsync(new List<Participant>()); // Niciun participant în conversație

            this._mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync("GhostUser"))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), Username = "GhostUser" });

            var result = await this._mentionService.ExtractMentionedUserIdsAsync(conversationId, content);

            Assert.Empty(result);
        }
    }
}