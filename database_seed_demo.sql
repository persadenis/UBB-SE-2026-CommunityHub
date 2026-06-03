USE [ChatAndEventsDB];
GO

SET NOCOUNT ON;

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
END;
GO

DECLARE @PasswordHash nvarchar(300) = '$2a$11$Y6.rMPmCDvKD8ZQuOwaIWuduKqPFrm9rF4gGhPUyoFXqJy7cDp1Yu';
DECLARE @Now datetime2 = SYSUTCDATETIME();
DECLARE @Administrator uniqueidentifier = '5E29A2CA-884F-4FEF-B730-2615DE6E4E0A';

DECLARE @DemoUsers TABLE (
    Id uniqueidentifier PRIMARY KEY,
    Username nvarchar(80),
    DisplayName nvarchar(120),
    Email nvarchar(180),
    AvatarUrl nvarchar(500),
    Bio nvarchar(500),
    ReputationPoints int,
    Birthday datetime2,
    Phone nvarchar(40)
);

INSERT INTO @DemoUsers VALUES
(@Administrator, 'administrator', 'Administrator', 'administrator@example.com', '/uploads/matchmaking/demo/administrator.png', 'Testing the full Community Hub integration.', 450, '2001-02-12', '0700000000'),
('11111111-1111-1111-1111-111111111111', 'ana_demo', 'Ana Demo', 'ana.demo@example.com', '/uploads/matchmaking/demo/ana.png', 'UX, music events and cozy community nights.', 320, '2002-03-08', '0711111111'),
('22222222-2222-2222-2222-222222222222', 'mara_demo', 'Mara Demo', 'mara.demo@example.com', '/uploads/matchmaking/demo/mara.png', 'Frontend, books and weekend workshops.', 280, '2003-07-19', '0722222222'),
('33333333-3333-3333-3333-333333333333', 'alex_demo', 'Alex Demo', 'alex.demo@example.com', '/uploads/matchmaking/demo/alex.png', 'Backend, quests and football meetups.', 510, '2000-11-02', '0733333333'),
('44444444-4444-4444-4444-444444444444', 'irina_demo', 'Irina Demo', 'irina.demo@example.com', '/uploads/matchmaking/demo/ana.png', 'Photography, volunteering and tech talks.', 190, '2002-09-23', '0744444444'),
('55555555-5555-5555-5555-555555555555', 'vlad_demo', 'Vlad Demo', 'vlad.demo@example.com', '/uploads/matchmaking/demo/alex.png', 'Gaming, hiking and event planning.', 610, '1999-12-14', '0755555555'),
('66666666-6666-6666-6666-666666666666', 'elena_demo', 'Elena Demo', 'elena.demo@example.com', '/uploads/matchmaking/demo/mara.png', 'Design systems, board games and coffee.', 410, '2001-05-30', '0766666666'),
('77777777-7777-7777-7777-777777777777', 'mihai_demo', 'Mihai Demo', 'mihai.demo@example.com', '/uploads/matchmaking/demo/alex.png', 'Data, basketball and hackathons.', 360, '2000-04-17', '0777777777'),
('88888888-8888-8888-8888-888888888888', 'sofia_demo', 'Sofia Demo', 'sofia.demo@example.com', '/uploads/matchmaking/demo/ana.png', 'Art, music festivals and community quests.', 240, '2004-01-26', '0788888888'),
('99999999-9999-9999-9999-999999999999', 'matei_demo', 'Matei Demo', 'matei.demo@example.com', '/uploads/matchmaking/demo/alex.png', 'DevOps, chess and chill events.', 390, '2001-08-11', '0799999999');

MERGE Users AS target
USING @DemoUsers AS source
ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET Username = source.Username, Email = source.Email, PasswordHash = @PasswordHash,
        AvatarUrl = source.AvatarUrl, Bio = source.Bio, Status = 0,
        Birthday = source.Birthday, Phone = source.Phone
WHEN NOT MATCHED THEN
    INSERT (Id, Username, Email, PasswordHash, AvatarUrl, Bio, Status, Birthday, Phone)
    VALUES (source.Id, source.Username, source.Email, @PasswordHash, source.AvatarUrl, source.Bio, 0, source.Birthday, source.Phone);

MERGE [User] AS target
USING @DemoUsers AS source
ON target.UserId = source.Id
WHEN MATCHED THEN
    UPDATE SET [Name] = source.Username, ReputationPoints = source.ReputationPoints
WHEN NOT MATCHED THEN
    INSERT (UserId, [Name], ReputationPoints)
    VALUES (source.Id, source.Username, source.ReputationPoints);

IF NOT EXISTS (SELECT 1 FROM Category WHERE CategoryId = 1)
    INSERT INTO Category (CategoryId, Title) VALUES (1, 'NATURE'), (2, 'FITNESS'), (3, 'MUSIC'), (4, 'SOCIAL'), (5, 'ART'), (6, 'PETS'), (7, 'TECH'), (8, 'FUN');

DECLARE @Dating TABLE (
    UserId uniqueidentifier PRIMARY KEY,
    DisplayName nvarchar(120),
    Gender nvarchar(60),
    PreferredGenders nvarchar(160),
    [Location] nvarchar(160),
    DatingBio nvarchar(1000),
    LoverType nvarchar(80),
    Interests nvarchar(500),
    PhotoUrl nvarchar(1000)
);

INSERT INTO @Dating VALUES
(@Administrator, 'Administrator', 'Man', 'Everyone', 'Cluj-Napoca', 'Here to test the app and meet people who like tech, events and community projects.', 'Community Builder', 'tech, music, board games, volunteering', '/uploads/matchmaking/demo/administrator.png'),
('11111111-1111-1111-1111-111111111111', 'Ana Demo', 'Woman', 'Men', 'Cluj-Napoca', 'I like live music, product design and small communities where everyone knows each other.', 'Creative Partner', 'music, design, coffee, tech', '/uploads/matchmaking/demo/ana.png'),
('22222222-2222-2222-2222-222222222222', 'Mara Demo', 'Woman', 'Everyone', 'Timisoara', 'Frontend person who likes books, workshops and relaxed conversations after events.', 'Slow Burn', 'frontend, books, workshops, coffee', '/uploads/matchmaking/demo/mara.png'),
('33333333-3333-3333-3333-333333333333', 'Alex Demo', 'Man', 'Everyone', 'Brasov', 'Backend, quests, sports and mountain trips.', 'Adventure Match', 'backend, quests, hiking, football', '/uploads/matchmaking/demo/alex.png'),
('44444444-4444-4444-4444-444444444444', 'Irina Demo', 'Woman', 'Everyone', 'Bucuresti', 'Photography, volunteering and late-night tech talks.', 'Curious Connector', 'photography, volunteering, tech, art', '/uploads/matchmaking/demo/ana.png'),
('55555555-5555-5555-5555-555555555555', 'Vlad Demo', 'Man', 'Women', 'Cluj-Napoca', 'Gaming, hiking and organizing chaotic but fun meetups.', 'Event Buddy', 'gaming, hiking, events, music', '/uploads/matchmaking/demo/alex.png'),
('66666666-6666-6666-6666-666666666666', 'Elena Demo', 'Woman', 'Everyone', 'Iasi', 'Design systems, board games and specialty coffee.', 'Cozy Match', 'design, board games, coffee, frontend', '/uploads/matchmaking/demo/mara.png'),
('77777777-7777-7777-7777-777777777777', 'Mihai Demo', 'Man', 'Everyone', 'Oradea', 'Data, basketball and student hackathons.', 'High Energy', 'data, basketball, hackathons, tech', '/uploads/matchmaking/demo/alex.png'),
('88888888-8888-8888-8888-888888888888', 'Sofia Demo', 'Woman', 'Men', 'Sibiu', 'Art, music festivals and quests that get people talking.', 'Festival Match', 'art, music, festivals, quests', '/uploads/matchmaking/demo/ana.png'),
('99999999-9999-9999-9999-999999999999', 'Matei Demo', 'Man', 'Everyone', 'Cluj-Napoca', 'DevOps, chess and low-key hangouts.', 'Calm Match', 'devops, chess, coffee, tech', '/uploads/matchmaking/demo/alex.png');

MERGE DatingProfiles AS target
USING @Dating AS source
ON target.UserId = source.UserId
WHEN MATCHED THEN
    UPDATE SET IsEnabled = 1, DisplayName = source.DisplayName, Gender = source.Gender,
        PreferredGenders = source.PreferredGenders, [Location] = source.[Location],
        DatingBio = source.DatingBio, MinPreferredAge = 18, MaxPreferredAge = 35,
        MaxDistanceKm = 80, LoverType = source.LoverType, IsArchived = 0, UpdatedAt = @Now
WHEN NOT MATCHED THEN
    INSERT (UserId, IsEnabled, DisplayName, Gender, PreferredGenders, [Location], DatingBio, MinPreferredAge, MaxPreferredAge, MaxDistanceKm, LoverType, IsArchived, CreatedAt, UpdatedAt)
    VALUES (source.UserId, 1, source.DisplayName, source.Gender, source.PreferredGenders, source.[Location], source.DatingBio, 18, 35, 80, source.LoverType, 0, @Now, @Now);

DELETE FROM DatingInterests WHERE UserId IN (SELECT Id FROM @DemoUsers);
INSERT INTO DatingInterests (UserId, [Name])
SELECT d.UserId, LTRIM(RTRIM(value))
FROM @Dating d
CROSS APPLY STRING_SPLIT(d.Interests, ',');

DELETE FROM DatingPhotos WHERE UserId IN (SELECT Id FROM @DemoUsers);
INSERT INTO DatingPhotos (UserId, Url, SortOrder)
SELECT UserId, PhotoUrl, 0 FROM @Dating;

DECLARE @Communities TABLE (Id uniqueidentifier PRIMARY KEY, [Name] nvarchar(120), [Description] nvarchar(1000), Category nvarchar(80), OwnerId uniqueidentifier);
INSERT INTO @Communities VALUES
('c0010001-0000-0000-0000-000000000001', 'Weaponized Penguins Team', 'Project room for the Community Hub app, UI ideas and integration notes.', 'Tech', @Administrator),
('c0010002-0000-0000-0000-000000000002', 'Cluj Tech Hangout', 'Casual meetups for students who want to build and test things together.', 'Tech', '33333333-3333-3333-3333-333333333333'),
('c0010003-0000-0000-0000-000000000003', 'Board Game Nights', 'Weekly board game and social nights.', 'Social', '66666666-6666-6666-6666-666666666666'),
('c0010004-0000-0000-0000-000000000004', 'Music Hunters', 'Find concerts, small gigs and festival friends.', 'Music', '11111111-1111-1111-1111-111111111111'),
('c0010005-0000-0000-0000-000000000005', 'Quest Builders', 'People who create and approve XP quests for events.', 'Events', '55555555-5555-5555-5555-555555555555'),
('c0010006-0000-0000-0000-000000000006', 'Study Sprint Club', 'Focused study sessions and accountability.', 'Education', '22222222-2222-2222-2222-222222222222');

MERGE HubCommunities AS target
USING @Communities AS source
ON target.[Name] = source.[Name]
WHEN MATCHED THEN
    UPDATE SET [Description] = source.[Description], Category = source.Category, OwnerId = source.OwnerId
WHEN NOT MATCHED THEN
    INSERT (Id, [Name], [Description], Category, OwnerId, CreatedAt)
    VALUES (source.Id, source.[Name], source.[Description], source.Category, source.OwnerId, @Now);

DECLARE @CommunityIds TABLE ([Name] nvarchar(120) PRIMARY KEY, Id uniqueidentifier, OwnerId uniqueidentifier);
INSERT INTO @CommunityIds
SELECT hc.[Name], hc.Id, c.OwnerId
FROM HubCommunities hc
JOIN @Communities c ON c.[Name] = hc.[Name];

INSERT INTO CommunityMemberships (CommunityId, UserId, IsAdmin, JoinedAt)
SELECT c.Id, u.Id, CASE WHEN c.OwnerId = u.Id THEN 1 ELSE 0 END, DATEADD(day, -ABS(CHECKSUM(NEWID())) % 20, @Now)
FROM @CommunityIds c
CROSS JOIN @DemoUsers u
WHERE (c.[Name] IN ('Weaponized Penguins Team', 'Cluj Tech Hangout') OR u.Id IN (@Administrator, c.OwnerId))
AND NOT EXISTS (SELECT 1 FROM CommunityMemberships m WHERE m.CommunityId = c.Id AND m.UserId = u.Id);

MERGE CommunityPosts AS target
USING (VALUES
('d0010001-0000-0000-0000-000000000001', 'Weaponized Penguins Team', @Administrator, 'Demo database is ready', 'This seed adds users, friends, events, quests, communities and matchmaking data.'),
('d0010002-0000-0000-0000-000000000002', 'Cluj Tech Hangout', '33333333-3333-3333-3333-333333333333', 'Friday testing meetup', 'Bring laptops. We will test login, events, chat and matchmaking.'),
('d0010003-0000-0000-0000-000000000003', 'Music Hunters', '11111111-1111-1111-1111-111111111111', 'Concert buddies needed', 'Looking for people who want to join the next local music event.')
) AS source (Id, CommunityName, AuthorId, Title, Body)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Title = source.Title, Body = source.Body
WHEN NOT MATCHED THEN INSERT (Id, CommunityId, AuthorId, Title, Body, CreatedAt)
VALUES (source.Id, (SELECT Id FROM @CommunityIds WHERE [Name] = source.CommunityName), source.AuthorId, source.Title, source.Body, @Now);

MERGE Friends AS target
USING (VALUES
('f0010001-0000-0000-0000-000000000001', @Administrator, '11111111-1111-1111-1111-111111111111', 1, 0),
('f0010002-0000-0000-0000-000000000002', @Administrator, '22222222-2222-2222-2222-222222222222', 1, 0),
('f0010003-0000-0000-0000-000000000003', @Administrator, '55555555-5555-5555-5555-555555555555', 1, 0),
('f0010004-0000-0000-0000-000000000004', '44444444-4444-4444-4444-444444444444', @Administrator, 0, 0),
('f0010005-0000-0000-0000-000000000005', @Administrator, '33333333-3333-3333-3333-333333333333', 0, 0)
) AS source (Id, UserId1, UserId2, [Status], IsMatch)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET UserId1 = source.UserId1, UserId2 = source.UserId2, [Status] = source.[Status], IsMatch = source.IsMatch
WHEN NOT MATCHED THEN INSERT (Id, UserId1, UserId2, [Status], IsMatch, CreatedAt) VALUES (source.Id, source.UserId1, source.UserId2, source.[Status], source.IsMatch, DATEADD(day, -3, @Now));

MERGE Conversations AS target
USING (VALUES
('a0010001-0000-0000-0000-000000000001', 0, NULL, NULL, @Administrator, 'Ana: See you at the music quest?', DATEADD(hour, -4, @Now)),
('a0010002-0000-0000-0000-000000000002', 0, NULL, NULL, @Administrator, 'Mara: I pushed the UI notes.', DATEADD(hour, -7, @Now)),
('a0010003-0000-0000-0000-000000000003', 0, NULL, NULL, @Administrator, 'Vlad: Board games tonight?', DATEADD(day, -1, @Now))
) AS source (Id, [Type], Title, IconUrl, CreatedBy, LastMessagePreview, LastMessageAt)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET LastMessagePreview = source.LastMessagePreview, LastMessageAt = source.LastMessageAt
WHEN NOT MATCHED THEN INSERT (Id, [Type], Title, IconUrl, CreatedBy, PinnedMessageId, LastMessagePreview, LastMessageAt, UnreadCount)
VALUES (source.Id, source.[Type], source.Title, source.IconUrl, source.CreatedBy, NULL, source.LastMessagePreview, source.LastMessageAt, 0);

INSERT INTO Participants (Id, ConversationId, UserId, JoinedAt, [Role], LastReadMessageId, TimeoutUntil, IsFavourite, IsNew, Nickname)
SELECT NEWID(), v.ConversationId, v.UserId, DATEADD(day, -5, @Now), 1, NULL, NULL, 0, 0, NULL
FROM (VALUES
('a0010001-0000-0000-0000-000000000001', @Administrator), ('a0010001-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111111'),
('a0010002-0000-0000-0000-000000000002', @Administrator), ('a0010002-0000-0000-0000-000000000002', '22222222-2222-2222-2222-222222222222'),
('a0010003-0000-0000-0000-000000000003', @Administrator), ('a0010003-0000-0000-0000-000000000003', '55555555-5555-5555-5555-555555555555')
) AS v(ConversationId, UserId)
WHERE NOT EXISTS (SELECT 1 FROM Participants p WHERE p.ConversationId = v.ConversationId AND p.UserId = v.UserId);

MERGE Messages AS target
USING (VALUES
('b0010001-0000-0000-0000-000000000001', 'a0010001-0000-0000-0000-000000000001', '11111111-1111-1111-1111-111111111111', 'See you at the music quest?', DATEADD(hour, -4, @Now)),
('b0010002-0000-0000-0000-000000000002', 'a0010001-0000-0000-0000-000000000001', @Administrator, 'Yes, I want to test event attendance too.', DATEADD(hour, -3, @Now)),
('b0010003-0000-0000-0000-000000000003', 'a0010002-0000-0000-0000-000000000002', '22222222-2222-2222-2222-222222222222', 'I pushed the UI notes.', DATEADD(hour, -7, @Now)),
('b0010004-0000-0000-0000-000000000004', 'a0010003-0000-0000-0000-000000000003', '55555555-5555-5555-5555-555555555555', 'Board games tonight?', DATEADD(day, -1, @Now))
) AS source (Id, ConversationId, UserId, Content, CreatedAt)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Content = source.Content
WHEN NOT MATCHED THEN INSERT (Id, ConversationId, UserId, Content, CreatedAt, ReplyToId, IsEdited, IsDeleted, MessageType, ParentMessageId, AttachmentImagePath, PinExpiresAt, AttachmentUrl, LinkPreviewTitle, LinkPreviewDesc)
VALUES (source.Id, source.ConversationId, source.UserId, source.Content, source.CreatedAt, NULL, 0, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL);

DECLARE @Events TABLE ([Name] nvarchar(200), [Location] nvarchar(160), StartDateTime datetime2, EndDateTime datetime2, [Description] nvarchar(max), MaximumPeople int, AdminId uniqueidentifier, CategoryId int);
INSERT INTO @Events VALUES
('Community Hub Launch Night', 'Cluj-Napoca', DATEADD(day, 3, @Now), DATEADD(day, 3, DATEADD(hour, 3, @Now)), 'Demo launch event with quests, discussions and memories.', 80, @Administrator, 7),
('Music Quest Weekend', 'Sibiu', DATEADD(day, 7, @Now), DATEADD(day, 7, DATEADD(hour, 5, @Now)), 'A social music event with XP quests for finding local artists.', 120, '11111111-1111-1111-1111-111111111111', 3),
('Board Games Social', 'Cluj-Napoca', DATEADD(day, 10, @Now), DATEADD(day, 10, DATEADD(hour, 4, @Now)), 'Casual board game night for friends and new members.', 40, '66666666-6666-6666-6666-666666666666', 4),
('Mountain Cleanup Quest', 'Brasov', DATEADD(day, 14, @Now), DATEADD(day, 14, DATEADD(hour, 6, @Now)), 'Nature volunteering event with reputation rewards.', 60, '33333333-3333-3333-3333-333333333333', 1),
('Student Hack Sprint', 'Timisoara', DATEADD(day, 20, @Now), DATEADD(day, 20, DATEADD(hour, 8, @Now)), 'Build a small feature in teams and submit quest proof.', 100, '22222222-2222-2222-2222-222222222222', 7);

INSERT INTO Events ([Name], [Location], StartDateTime, EndDateTime, IsPublic, [Description], MaximumPeople, EventBannerPath, SlowModeSeconds, EnrolledCount, AdminId, CategoryId)
SELECT e.[Name], e.[Location], e.StartDateTime, e.EndDateTime, 1, e.[Description], e.MaximumPeople, NULL, 5, 0, e.AdminId, e.CategoryId
FROM @Events e
WHERE NOT EXISTS (SELECT 1 FROM Events existing WHERE existing.[Name] = e.[Name]);

DECLARE @EventIds TABLE (EventId int, [Name] nvarchar(200));
INSERT INTO @EventIds
SELECT EventId, [Name] FROM Events WHERE [Name] IN (SELECT [Name] FROM @Events);

INSERT INTO Quests ([Name], [Description], Difficulty, PrerequisiteQuestId, EventId)
SELECT q.[Name], q.[Description], q.Difficulty, NULL, e.EventId
FROM @EventIds e
CROSS APPLY (VALUES
('Check in', 'Join the event and say hello in the discussion.', 1),
('Make a memory', 'Upload or describe one memory from the event.', 2),
('Help someone new', 'Talk to a new participant and help them find a group.', 3)
) q([Name], [Description], Difficulty)
WHERE NOT EXISTS (SELECT 1 FROM Quests existing WHERE existing.EventId = e.EventId AND existing.[Name] = q.[Name]);

INSERT INTO AttendedEvents (EventId, UserId, EnrollmentDate, IsArchived, IsFavourite, UnreadAnnouncementCount)
SELECT e.EventId, u.Id, DATEADD(day, -1, @Now), 0, CASE WHEN u.Id = @Administrator THEN 1 ELSE 0 END, CASE WHEN u.Id = @Administrator THEN 2 ELSE 0 END
FROM @EventIds e
JOIN @DemoUsers u ON u.Id IN (@Administrator, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333333')
WHERE NOT EXISTS (SELECT 1 FROM AttendedEvents ae WHERE ae.EventId = e.EventId AND ae.UserId = u.Id);

UPDATE ev
SET EnrolledCount = counts.Total
FROM Events ev
JOIN (
    SELECT EventId, COUNT(*) AS Total
    FROM AttendedEvents
    GROUP BY EventId
) counts ON counts.EventId = ev.EventId;

DELETE FROM DatingSwipes
WHERE FromUserId IN (SELECT Id FROM @DemoUsers)
   OR ToUserId IN (SELECT Id FROM @DemoUsers);

INSERT INTO DatingSwipes (FromUserId, ToUserId, [Action], CreatedAt) VALUES
(@Administrator, '11111111-1111-1111-1111-111111111111', 'Like', DATEADD(hour, -5, @Now)),
('11111111-1111-1111-1111-111111111111', @Administrator, 'Like', DATEADD(hour, -6, @Now)),
(@Administrator, '33333333-3333-3333-3333-333333333333', 'Like', DATEADD(hour, -3, @Now)),
('33333333-3333-3333-3333-333333333333', @Administrator, 'SuperLike', DATEADD(hour, -2, @Now)),
('88888888-8888-8888-8888-888888888888', @Administrator, 'Like', DATEADD(hour, -1, @Now));

INSERT INTO Notifications (UserId, Title, [Description], CreatedAt, IsRead, [Type], SourceFeature, SourceEntityId)
SELECT n.UserId, n.Title, n.[Description], @Now, 0, n.[Type], n.SourceFeature, n.SourceEntityId
FROM (VALUES
(@Administrator, 'New friend request', 'Irina Demo sent you a friend request.', 'FriendRequest', 'Friends', '44444444-4444-4444-4444-444444444444'),
(@Administrator, 'New matchmaking match', 'You matched with Ana Demo.', 'Matchmaking', 'Matchmaking', '11111111-1111-1111-1111-111111111111'),
(@Administrator, 'Upcoming event', 'Community Hub Launch Night starts soon.', 'Event', 'Events', 'Community Hub Launch Night'),
(@Administrator, 'Quest available', 'New quests are available in your enrolled events.', 'Quest', 'Events', 'QuestSeed')
) n(UserId, Title, [Description], [Type], SourceFeature, SourceEntityId)
WHERE NOT EXISTS (
    SELECT 1 FROM Notifications existing
    WHERE existing.UserId = n.UserId AND existing.Title = n.Title AND existing.SourceEntityId = n.SourceEntityId
);

SELECT 'Users' AS TableName, COUNT(*) AS CountValue FROM Users
UNION ALL SELECT 'Friends', COUNT(*) FROM Friends
UNION ALL SELECT 'Conversations', COUNT(*) FROM Conversations
UNION ALL SELECT 'Messages', COUNT(*) FROM Messages
UNION ALL SELECT 'Events', COUNT(*) FROM Events
UNION ALL SELECT 'Quests', COUNT(*) FROM Quests
UNION ALL SELECT 'HubCommunities', COUNT(*) FROM HubCommunities
UNION ALL SELECT 'DatingProfiles', COUNT(*) FROM DatingProfiles
UNION ALL SELECT 'DatingSwipes', COUNT(*) FROM DatingSwipes
UNION ALL SELECT 'Notifications', COUNT(*) FROM Notifications;
