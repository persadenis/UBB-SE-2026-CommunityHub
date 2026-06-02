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
    public class SearchServiceTests
    {
        private readonly Mock<IMessageRepository> _mockMessageRepo;
        private readonly Mock<IParticipantRepository> _mockPartRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly SearchService _searchService;

        public SearchServiceTests()
        {
            _mockMessageRepo = new Mock<IMessageRepository>();
            _mockPartRepo = new Mock<IParticipantRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            _searchService = new SearchService(
                _mockMessageRepo.Object,
                _mockPartRepo.Object,
                _mockUserRepo.Object
            );
        }

        [Fact]
        public void Constructor_NullMessageRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SearchService(null!, _mockPartRepo.Object, _mockUserRepo.Object));
        }

        [Fact]
        public void Constructor_NullParticipantRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SearchService(_mockMessageRepo.Object, null!, _mockUserRepo.Object));
        }

        [Fact]
        public void Constructor_NullUserRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SearchService(_mockMessageRepo.Object, _mockPartRepo.Object, null!));
        }

        [Fact]
        public async Task SearchMessagesAsync_RequesterNotParticipant_ThrowsInvalidOperationException()
        {
            var convoId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _mockPartRepo.Setup(r => r.GetAsync(convoId, userId)).ReturnsAsync((Participant?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _searchService.SearchMessagesAsync(convoId, userId, "query")
            );
        }

        [Fact]
        public async Task SearchMessagesAsync_SystemMessage_AssignsSystemUsername()
        {
            var convoId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            SetupSearchContext(convoId, userId, new List<Message> { new Message { UserId = null } });

            var result = await _searchService.SearchMessagesAsync(convoId, userId, "test");

            Assert.Equal("System", result[0].SenderUsername);
        }

        [Fact]
        public async Task SearchMessagesAsync_KnownUser_AssignsUsernameFromRepo()
        {
            var convoId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            SetupSearchContext(convoId, userId, new List<Message> { new Message { UserId = senderId } });
            _mockUserRepo.Setup(r => r.GetByIdAsync(senderId)).ReturnsAsync(new User { Username = "KnownUser" });

            var result = await _searchService.SearchMessagesAsync(convoId, userId, "test");

            Assert.Equal("KnownUser", result[0].SenderUsername);
        }

        [Fact]
        public async Task SearchMessagesAsync_UnknownUser_AssignsDefaultUsername()
        {
            var convoId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            SetupSearchContext(convoId, userId, new List<Message> { new Message { UserId = senderId } });
            _mockUserRepo.Setup(r => r.GetByIdAsync(senderId)).ReturnsAsync((User?)null);

            var result = await _searchService.SearchMessagesAsync(convoId, userId, "test");

           
            Assert.Equal("Unknown User", result[0].SenderUsername);
        }


        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task SearchUsersAsync_InvalidQuery_ReturnsEmptyList(string query)
        {
            var result = await _searchService.SearchUsersAsync(query!);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchUsersAsync_ValidQuery_ReturnsSingleResult()
        {
            _mockUserRepo.Setup(r => r.SearchByUsernameAsync("John")).ReturnsAsync(new List<User> { new User { Username = "John" } });

            var result = await _searchService.SearchUsersAsync("John");

            Assert.Single(result);
        }


        [Fact]
        public async Task SearchUsersForAddMemberAsync_FiltersExistingUsers()
        {
            var convoId = Guid.NewGuid();
            var existingId = Guid.NewGuid();
            var newId = Guid.NewGuid();

            _mockPartRepo.Setup(r => r.GetAllForConversationAsync(convoId)).ReturnsAsync(new List<Participant> { new Participant { UserId = existingId } });
            _mockUserRepo.Setup(r => r.SearchByUsernameAsync("test")).ReturnsAsync(new List<User>
            {
                new User { Id = existingId },
                new User { Id = newId }
            });

            var result = await _searchService.SearchUsersForAddMemberAsync(convoId, "test");

            Assert.All(result, user => Assert.NotEqual(existingId, user.Id));
        }

        private void SetupSearchContext(Guid convoId, Guid userId, List<Message> messages)
        {
            _mockPartRepo.Setup(r => r.GetAsync(convoId, userId)).ReturnsAsync(new Participant());
            _mockMessageRepo.Setup(r => r.SearchInConversationAsync(convoId, It.IsAny<string>())).ReturnsAsync(messages);
        }
    }
}