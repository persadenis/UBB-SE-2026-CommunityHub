using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using Microsoft.Data.SqlClient;

namespace ChatAndEvents.Data.ChatData.repositories
{
    /// <summary>
    /// Provides methods for accessing and retrieving chat-related data from the database.
    /// </summary>
    public class DatabaseManager
    {
        /// <summary>
        /// Gets the connection string used for database access.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseManager"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        public DatabaseManager(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets all users from the database.
        /// </summary>
        /// <returns>A list of users.</returns>
        public async Task<List<User>> GetUsersAsync()
        {
            var users = new List<User>();

            await using var connection = new SqlConnection(this.ConnectionString);
            await connection.OpenAsync();

            const string Sql = "SELECT Id, Username, Email, PasswordHash, AvatarUrl, Bio, Status, Birthday, Phone FROM Users;";
            await using var command = new SqlCommand(Sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = (Guid)reader["Id"],
                    Username = (string)reader["Username"],
                    Email = (string)reader["Email"],
                    PasswordHash = (string)reader["PasswordHash"],
                    AvatarUrl = reader["AvatarUrl"] as string,
                    Bio = reader["Bio"] as string,
                    Status = (UserStatus)(byte)reader["Status"],
                    Birthday = reader["Birthday"] != DBNull.Value ? (DateTime)reader["Birthday"] : null,
                    Phone = reader["Phone"] as string
                });
            }

            return users;
        }

        /// <summary>
        /// Gets all messages from the database.
        /// </summary>
        /// <returns>A list of messages.</returns>
        public async Task<List<Message>> GetMessagesAsync()
        {
            var messages = new List<Message>();

            await using var connection = new SqlConnection(this.ConnectionString);
            await connection.OpenAsync();

            const string Sql = "SELECT Id, ConversationId, UserId, Content, CreatedAt, ReplyToId, IsEdited, IsDeleted, MessageType, ParentMessageId FROM Messages;";
            await using var command = new SqlCommand(Sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                messages.Add(new Message
                {
                    Id = (Guid)reader["Id"],
                    ConversationId = (Guid)reader["ConversationId"],
                    UserId = reader["UserId"] != DBNull.Value ? (Guid)reader["UserId"] : null,
                    Content = reader["Content"] as string,
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    ReplyToId = reader["ReplyToId"] != DBNull.Value ? (Guid)reader["ReplyToId"] : null,
                    IsEdited = (bool)reader["IsEdited"],
                    IsDeleted = (bool)reader["IsDeleted"],
                    MessageType = (MessageType)(byte)reader["MessageType"],
                    ParentMessageId = reader["ParentMessageId"] != DBNull.Value ? (Guid)reader["ParentMessageId"] : null,
                });
            }

            return messages;
        }

        /// <summary>
        /// Gets all conversations from the database.
        /// </summary>
        /// <returns>A list of conversations.</returns>
        public async Task<List<Conversation>> GetConversationsAsync()
        {
            var conversations = new List<Conversation>();

            await using var connection = new SqlConnection(this.ConnectionString);
            await connection.OpenAsync();

            const string Sql = "SELECT Id, Type, Title, IconUrl, CreatedBy, PinnedMessageId FROM Conversations;";
            await using var command = new SqlCommand(Sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                conversations.Add(new Conversation
                {
                    Id = (Guid)reader["Id"],
                    Type = (ConversationType)(byte)reader["Type"],
                    Title = reader["Title"] as string,
                    IconUrl = reader["IconUrl"] as string,
                    CreatedBy = (Guid)reader["CreatedBy"],
                    PinnedMessageId = reader["PinnedMessageId"] != DBNull.Value ? (Guid)reader["PinnedMessageId"] : null,
                });
            }

            return conversations;
        }

        /// <summary>
        /// Gets all participants from the database.
        /// </summary>
        /// <returns>A list of participants.</returns>
        public async Task<List<Participant>> GetParticipantsAsync()
        {
            var participants = new List<Participant>();

            await using var connection = new SqlConnection(this.ConnectionString);
            await connection.OpenAsync();

            const string Sql = "SELECT Id, ConversationId, UserId, JoinedAt, Role, LastReadMessageId, TimeoutUntil, IsFavourite FROM Participants;";
            await using var command = new SqlCommand(Sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                participants.Add(new Participant
                {
                    Id = (Guid)reader["Id"],
                    ConversationId = (Guid)reader["ConversationId"],
                    UserId = (Guid)reader["UserId"],
                    JoinedAt = (DateTime)reader["JoinedAt"],
                    Role = (ParticipantRole)(byte)reader["Role"],
                    LastReadMessageId = reader["LastReadMessageId"] != DBNull.Value ? (Guid)reader["LastReadMessageId"] : null,
                    TimeoutUntil = reader["TimeoutUntil"] != DBNull.Value ? (DateTime)reader["TimeoutUntil"] : null,
                    IsFavourite = (bool)reader["IsFavourite"],
                });
            }

            return participants;
        }

        /// <summary>
        /// Gets all friends from the database.
        /// </summary>
        /// <returns>A list of friends.</returns>
        public async Task<List<Friend>> GetFriendsAsync()
        {
            var friends = new List<Friend>();

            await using var connection = new SqlConnection(this.ConnectionString);
            await connection.OpenAsync();

            const string Sql = "SELECT Id, UserId1, UserId2, Status, IsMatch, CreatedAt FROM Friends;";
            await using var command = new SqlCommand(Sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                friends.Add(new Friend
                {
                    Id = (Guid)reader["Id"],
                    UserId1 = (Guid)reader["UserId1"],
                    UserId2 = (Guid)reader["UserId2"],
                    Status = (FriendStatus)(byte)reader["Status"],
                    IsMatch = (bool)reader["IsMatch"],
                    CreatedAt = (DateTime)reader["CreatedAt"],
                });
            }

            return friends;
        }
    }
}