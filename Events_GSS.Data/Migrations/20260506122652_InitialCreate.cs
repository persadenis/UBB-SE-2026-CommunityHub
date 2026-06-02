using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAndEvents.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsUnlocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReputationPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Username = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaximumPeople = table.Column<int>(type: "int", nullable: true),
                    EventBannerPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SlowModeSeconds = table.Column<int>(type: "int", nullable: true),
                    EnrolledCount = table.Column<int>(type: "int", nullable: false),
                    AdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Events_User_AdminId",
                        column: x => x.AdminId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users_RP_scores",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReputationPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Tier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Newcomer")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_RP_scores", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_users_RP_scores_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId2 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsMatch = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friends_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friends_Users_UserId2",
                        column: x => x.UserId2,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsExpanded = table.Column<bool>(type: "bit", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.AnnouncementId);
                    table.ForeignKey(
                        name: "FK_Announcements_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Announcements_User_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AttendedEvents",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    IsFavourite = table.Column<bool>(type: "bit", nullable: false),
                    UnreadAnnouncementCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendedEvents", x => new { x.EventId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AttendedEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AttendedEvents_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DiscussionMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    AssociatedEventEventId = table.Column<int>(type: "int", nullable: true),
                    AuthorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReplyToId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscussionMessages_DiscussionMessages_ReplyToId",
                        column: x => x.ReplyToId,
                        principalTable: "DiscussionMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiscussionMessages_Events_AssociatedEventEventId",
                        column: x => x.AssociatedEventEventId,
                        principalTable: "Events",
                        principalColumn: "EventId");
                    table.ForeignKey(
                        name: "FK_DiscussionMessages_User_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Discussions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discussions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discussions_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Discussions_User_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Memories",
                columns: table => new
                {
                    MemoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memories", x => x.MemoryId);
                    table.ForeignKey(
                        name: "FK_Memories_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Memories_User_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteQuestId = table.Column<int>(type: "int", nullable: true),
                    EventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quests_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Quests_Quests_PrerequisiteQuestId",
                        column: x => x.PrerequisiteQuestId,
                        principalTable: "Quests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnnouncementReactions",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementReactions", x => new { x.AnnouncementId, x.AuthorId });
                    table.ForeignKey(
                        name: "FK_AnnouncementReactions_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "AnnouncementId");
                    table.ForeignKey(
                        name: "FK_AnnouncementReactions_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnnouncementReadReceipts",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementReadReceipts", x => new { x.AnnouncementId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AnnouncementReadReceipts_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "AnnouncementId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AnnouncementReadReceipts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "DiscussionMutes",
                columns: table => new
                {
                    DiscussionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    MutedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MutedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MutedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPermanent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionMutes", x => new { x.DiscussionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DiscussionMutes_Discussions_DiscussionId",
                        column: x => x.DiscussionId,
                        principalTable: "Discussions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiscussionMutes_User_MutedById",
                        column: x => x.MutedById,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_DiscussionMutes_User_MutedUserId",
                        column: x => x.MutedUserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "DiscussionReactions",
                columns: table => new
                {
                    DiscussionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionReactions", x => new { x.DiscussionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_DiscussionReactions_DiscussionMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "DiscussionMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiscussionReactions_Discussions_DiscussionId",
                        column: x => x.DiscussionId,
                        principalTable: "Discussions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiscussionReactions_User_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MemoryLikes",
                columns: table => new
                {
                    MemoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryLikes", x => new { x.MemoryId, x.UserId });
                    table.ForeignKey(
                        name: "FK_MemoryLikes_Memories_MemoryId",
                        column: x => x.MemoryId,
                        principalTable: "Memories",
                        principalColumn: "MemoryId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_MemoryLikes_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "QuestMemories",
                columns: table => new
                {
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    MemoryId = table.Column<int>(type: "int", nullable: false),
                    ProofStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestMemories", x => new { x.QuestId, x.MemoryId });
                    table.ForeignKey(
                        name: "FK_QuestMemories_Memories_MemoryId",
                        column: x => x.MemoryId,
                        principalTable: "Memories",
                        principalColumn: "MemoryId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_QuestMemories_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PinnedMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastMessagePreview = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnreadCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReplyToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    ParentMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AttachmentImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PinExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkPreviewTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LinkPreviewDesc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ParentMessageId",
                        column: x => x.ParentMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ReplyToId",
                        column: x => x.ReplyToId,
                        principalTable: "Messages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    LastReadMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TimeoutUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFavourite = table.Column<bool>(type: "bit", nullable: false),
                    IsNew = table.Column<bool>(type: "bit", nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Participants_Messages_LastReadMessageId",
                        column: x => x.LastReadMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Participants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementReactions_AuthorId",
                table: "AnnouncementReactions",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementReadReceipts_UserId",
                table: "AnnouncementReadReceipts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_AuthorId",
                table: "Announcements",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_EventId",
                table: "Announcements",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendedEvents_UserId",
                table: "AttendedEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatedBy",
                table: "Conversations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_PinnedMessageId",
                table: "Conversations",
                column: "PinnedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionMessages_AssociatedEventEventId",
                table: "DiscussionMessages",
                column: "AssociatedEventEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionMessages_AuthorUserId",
                table: "DiscussionMessages",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionMessages_ReplyToId",
                table: "DiscussionMessages",
                column: "ReplyToId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionMutes_MutedById",
                table: "DiscussionMutes",
                column: "MutedById");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionMutes_MutedUserId",
                table: "DiscussionMutes",
                column: "MutedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionReactions_AuthorId",
                table: "DiscussionReactions",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionReactions_MessageId",
                table: "DiscussionReactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Discussions_CreatorId",
                table: "Discussions",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Discussions_EventId",
                table: "Discussions",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_AdminId",
                table: "Events",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CategoryId",
                table: "Events",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId1_UserId2",
                table: "Friends",
                columns: new[] { "UserId1", "UserId2" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId2",
                table: "Friends",
                column: "UserId2");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_AuthorId",
                table: "Memories",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_EventId",
                table: "Memories",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryLikes_UserId",
                table: "MemoryLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_CreatedAt",
                table: "Messages",
                columns: new[] { "ConversationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ParentMessageId",
                table: "Messages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReplyToId",
                table: "Messages",
                column: "ReplyToId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ConversationId",
                table: "Participants",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ConversationId_UserId",
                table: "Participants",
                columns: new[] { "ConversationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_LastReadMessageId",
                table: "Participants",
                column: "LastReadMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_UserId",
                table: "Participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestMemories_MemoryId",
                table: "QuestMemories",
                column: "MemoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_EventId",
                table: "Quests",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_PrerequisiteQuestId",
                table: "Quests",
                column: "PrerequisiteQuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Messages_PinnedMessageId",
                table: "Conversations",
                column: "PinnedMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_CreatedBy",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Messages_PinnedMessageId",
                table: "Conversations");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "AnnouncementReactions");

            migrationBuilder.DropTable(
                name: "AnnouncementReadReceipts");

            migrationBuilder.DropTable(
                name: "AttendedEvents");

            migrationBuilder.DropTable(
                name: "DiscussionMutes");

            migrationBuilder.DropTable(
                name: "DiscussionReactions");

            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "MemoryLikes");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "QuestMemories");

            migrationBuilder.DropTable(
                name: "users_RP_scores");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "DiscussionMessages");

            migrationBuilder.DropTable(
                name: "Discussions");

            migrationBuilder.DropTable(
                name: "Memories");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Conversations");
        }
    }
}
