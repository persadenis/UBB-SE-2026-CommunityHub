using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.Database;

public static class CommunityHubDatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!db.Database.IsSqlServer())
        {
            return;
        }

        var statements = new[]
        {
            """
            IF COL_LENGTH('Notifications', 'IsRead') IS NULL
                ALTER TABLE Notifications ADD IsRead bit NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0;
            """,
            """
            IF COL_LENGTH('Notifications', 'Type') IS NULL
                ALTER TABLE Notifications ADD [Type] nvarchar(80) NOT NULL CONSTRAINT DF_Notifications_Type DEFAULT 'General';
            """,
            """
            IF COL_LENGTH('Notifications', 'SourceFeature') IS NULL
                ALTER TABLE Notifications ADD SourceFeature nvarchar(80) NOT NULL CONSTRAINT DF_Notifications_SourceFeature DEFAULT 'System';
            """,
            """
            IF COL_LENGTH('Notifications', 'SourceEntityId') IS NULL
                ALTER TABLE Notifications ADD SourceEntityId nvarchar(120) NULL;
            """,
            """
            IF OBJECT_ID('HubCommunities', 'U') IS NULL
            BEGIN
                CREATE TABLE HubCommunities (
                    Id uniqueidentifier NOT NULL PRIMARY KEY,
                    [Name] nvarchar(120) NOT NULL,
                    [Description] nvarchar(1000) NOT NULL,
                    Category nvarchar(80) NOT NULL,
                    BannerUrl nvarchar(1000) NULL,
                    OwnerId uniqueidentifier NOT NULL,
                    CreatedAt datetime2 NOT NULL,
                    CONSTRAINT FK_HubCommunities_Users_OwnerId FOREIGN KEY (OwnerId) REFERENCES Users(Id)
                );
                CREATE UNIQUE INDEX IX_HubCommunities_Name ON HubCommunities([Name]);
            END
            """,
            """
            IF COL_LENGTH('HubCommunities', 'BannerUrl') IS NULL
                ALTER TABLE HubCommunities ADD BannerUrl nvarchar(1000) NULL;
            """,
            """
            IF OBJECT_ID('CommunityMemberships', 'U') IS NULL
            BEGIN
                CREATE TABLE CommunityMemberships (
                    CommunityId uniqueidentifier NOT NULL,
                    UserId uniqueidentifier NOT NULL,
                    IsAdmin bit NOT NULL,
                    JoinedAt datetime2 NOT NULL,
                    CONSTRAINT PK_CommunityMemberships PRIMARY KEY (CommunityId, UserId),
                    CONSTRAINT FK_CommunityMemberships_HubCommunities_CommunityId FOREIGN KEY (CommunityId) REFERENCES HubCommunities(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_CommunityMemberships_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id)
                );
            END
            """,
            """
            IF OBJECT_ID('CommunityPosts', 'U') IS NULL
            BEGIN
                CREATE TABLE CommunityPosts (
                    Id uniqueidentifier NOT NULL PRIMARY KEY,
                    CommunityId uniqueidentifier NOT NULL,
                    AuthorId uniqueidentifier NOT NULL,
                    Title nvarchar(160) NOT NULL,
                    Body nvarchar(4000) NOT NULL,
                    CreatedAt datetime2 NOT NULL,
                    CONSTRAINT FK_CommunityPosts_HubCommunities_CommunityId FOREIGN KEY (CommunityId) REFERENCES HubCommunities(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_CommunityPosts_Users_AuthorId FOREIGN KEY (AuthorId) REFERENCES Users(Id)
                );
            END
            """,
            """
            IF OBJECT_ID('DatingProfiles', 'U') IS NULL
            BEGIN
                CREATE TABLE DatingProfiles (
                    UserId uniqueidentifier NOT NULL PRIMARY KEY,
                    IsEnabled bit NOT NULL,
                    DisplayName nvarchar(120) NOT NULL,
                    Gender nvarchar(60) NOT NULL,
                    PreferredGenders nvarchar(160) NOT NULL,
                    [Location] nvarchar(160) NOT NULL,
                    DatingBio nvarchar(1000) NOT NULL,
                    MinPreferredAge int NOT NULL,
                    MaxPreferredAge int NOT NULL,
                    MaxDistanceKm int NOT NULL,
                    LoverType nvarchar(80) NOT NULL,
                    IsArchived bit NOT NULL,
                    CreatedAt datetime2 NOT NULL,
                    UpdatedAt datetime2 NOT NULL,
                    CONSTRAINT FK_DatingProfiles_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                );
            END
            """,
            """
            IF OBJECT_ID('DatingInterests', 'U') IS NULL
            BEGIN
                CREATE TABLE DatingInterests (
                    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    UserId uniqueidentifier NOT NULL,
                    [Name] nvarchar(80) NOT NULL,
                    CONSTRAINT FK_DatingInterests_DatingProfiles_UserId FOREIGN KEY (UserId) REFERENCES DatingProfiles(UserId) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX IX_DatingInterests_UserId_Name ON DatingInterests(UserId, [Name]);
            END
            """,
            """
            IF OBJECT_ID('DatingPhotos', 'U') IS NULL
            BEGIN
                CREATE TABLE DatingPhotos (
                    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    UserId uniqueidentifier NOT NULL,
                    Url nvarchar(1000) NOT NULL,
                    SortOrder int NOT NULL,
                    CONSTRAINT FK_DatingPhotos_DatingProfiles_UserId FOREIGN KEY (UserId) REFERENCES DatingProfiles(UserId) ON DELETE CASCADE
                );
            END
            """,
            """
            IF OBJECT_ID('DatingSwipes', 'U') IS NULL
            BEGIN
                CREATE TABLE DatingSwipes (
                    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    FromUserId uniqueidentifier NOT NULL,
                    ToUserId uniqueidentifier NOT NULL,
                    [Action] nvarchar(20) NOT NULL,
                    CreatedAt datetime2 NOT NULL,
                    CONSTRAINT FK_DatingSwipes_Users_FromUserId FOREIGN KEY (FromUserId) REFERENCES Users(Id),
                    CONSTRAINT FK_DatingSwipes_Users_ToUserId FOREIGN KEY (ToUserId) REFERENCES Users(Id)
                );
                CREATE UNIQUE INDEX IX_DatingSwipes_FromUserId_ToUserId ON DatingSwipes(FromUserId, ToUserId);
            END
            """,
        };

        foreach (var statement in statements)
        {
            await db.Database.ExecuteSqlRawAsync(statement);
        }
    }
}
