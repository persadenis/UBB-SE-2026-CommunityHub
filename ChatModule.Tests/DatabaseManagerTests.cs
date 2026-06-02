using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.IO;
using ChatAndEvents.Data.ChatData.repositories;

namespace ChatModule.Tests
{
    public class DatabaseManagerTests
    {
        private readonly DatabaseManager _dbManager;

        public DatabaseManagerTests()
        {
            // Load connection string from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var connectionString = config.GetConnectionString("ChatModuleDb")
                ?? throw new System.InvalidOperationException("Connection string 'ChatModuleDb' not found in appsettings.json.");
            _dbManager = new DatabaseManager(connectionString);
        }

        [DbFact]
        public async Task GetUsersAsync_ReturnsUsers()
        {
            var users = await _dbManager.GetUsersAsync();
            Assert.NotNull(users);
            Assert.NotEmpty(users);
        }

        [DbFact]
        public async Task GetMessagesAsync_ReturnsMessages()
        {
            var messages = await _dbManager.GetMessagesAsync();
            Assert.NotNull(messages);
        }

        [DbFact]
        public async Task GetConversationsAsync_ReturnsConversations()
        {
            var conversations = await _dbManager.GetConversationsAsync();
            Assert.NotNull(conversations);
        }

        [DbFact]
        public async Task GetParticipantsAsync_ReturnsParticipants()
        {
            var participants = await _dbManager.GetParticipantsAsync();
            Assert.NotNull(participants);
        }

        [DbFact]
        public async Task GetFriendsAsync_ReturnsFriends()
        {
            var friends = await _dbManager.GetFriendsAsync();
            Assert.NotNull(friends);
        }
    }

    public class DbFactAttribute : FactAttribute
    {
        public DbFactAttribute()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();
                var conn = config.GetConnectionString("ChatModuleDb");
                if (string.IsNullOrEmpty(conn))
                {
                    Skip = "DB connection string not found. Skipping DB test.";
                }
            }
            catch
            {
                Skip = "appsettings.json not found or invalid. Skipping DB test.";
            }
        }
    }
}
