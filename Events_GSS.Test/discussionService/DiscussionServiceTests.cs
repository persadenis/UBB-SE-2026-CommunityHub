using System;
using System.Collections.Generic;
using System.Text;

using Moq;

using Xunit;
using Events_GSS.Data.Services.Interfaces;
using System.Security.Cryptography.X509Certificates;
using Windows.AI.MachineLearning.Preview;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.Extensions.Logging;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories.discussionRepository;
using ChatAndEvents.Data.EventsData.Repositories.eventRepository;
using ChatAndEvents.Data.EventsData.Services.discussionService;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;

namespace Events_GSS.Test.discussionService;

public class DiscussionServiceTests
{
    private Mock<IDiscussionRepository> mockRepo;
    private Mock<IEventRepository> mockEventRepo;
    private Mock<IReputationService> mockReputation;
    private Mock<INotificationService> mockNotification;
    private DiscussionService service;

    public DiscussionServiceTests()
    {
        mockRepo = new Mock<IDiscussionRepository>();
        mockEventRepo = new Mock<IEventRepository>();
        mockReputation = new Mock<IReputationService>();
        mockNotification = new Mock<INotificationService>();
        service = new DiscussionService(
            mockRepo.Object,
            mockEventRepo.Object,
            mockReputation.Object,
            mockNotification.Object);
    }

    private Event MakeEvent(int eventId, Guid? adminId = null, int? slowMode = null) =>
       new Event
       {
           EventId = eventId,
           Admin = adminId.HasValue ? new User { UserId = adminId.Value } : null,
           SlowModeSeconds = slowMode
       };

    [Fact]
    public async Task GetMessagesAsync_EventNotFound_ThrowsArgumentException()
    {
        mockEventRepo.Setup(e => e.GetByIdAsync(99)).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetMessagesAsync(99, Guid.Parse("00000000-0000-0000-0000-000000000001")));
    }

    [Fact]
    public async Task GetMessagesAsync_AdminUser_CanDeleteAllMessages()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000005");
        int eventId = 1;
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var messages = new List<DiscussionMessage>
        {
            new DiscussionMessage(1, "hello", DateTime.UtcNow) { Author = new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000006") } },
        };
        mockRepo.Setup(r => r.GetByEventAsync(eventId, adminId)).ReturnsAsync(messages);

        var result = await service.GetMessagesAsync(eventId, adminId);

        Assert.True(result[0].CanDelete);
    }

    [Fact]
    public async Task GetMessagesAsync_RegularUser_CanOnlyDeleteOwnMessages()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        int eventId = 1;
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var messages = new List<DiscussionMessage>
        {
            new DiscussionMessage(1, "mine",     DateTime.UtcNow) { Author = new User { UserId = userId } },
            new DiscussionMessage(2, "not mine", DateTime.UtcNow) { Author = new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000077") } }
        };
        mockRepo.Setup(r => r.GetByEventAsync(eventId, userId)).ReturnsAsync(messages);

        var result = await service.GetMessagesAsync(eventId, userId);

        Assert.True(result[0].CanDelete);
    }


    [Fact]
    public async Task GetMessagesAsync_RegularUser_CannotDeleteOthersMessages()
    {
        Guid randomPersonId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        Guid authorId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000004");
        int eventId = 1;

        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var messages = new List<DiscussionMessage>
    {
        new DiscussionMessage(1, "hello", DateTime.UtcNow) { Author = new User { UserId = authorId } }
    };
        mockRepo.Setup(r => r.GetByEventAsync(eventId, randomPersonId)).ReturnsAsync(messages);

        var result = await service.GetMessagesAsync(eventId, randomPersonId);


        Assert.False(result[0].CanDelete);
    }



    [Fact]
    public async Task GetMessagesAsync_MessageWithNullAuthor_RegularUserCannotDelete()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        int eventId = 1;
        var evt = MakeEvent(eventId, adminId: Guid.Parse("00000000-0000-0000-0000-000000000099"));
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);


        var messages = new List<DiscussionMessage>
        {
            new DiscussionMessage(1, "noauthormessage!", DateTime.UtcNow) { Author = null }
        };
        mockRepo.Setup(r => r.GetByEventAsync(eventId, userId)).ReturnsAsync(messages);

        var result = await service.GetMessagesAsync(eventId, userId);

        Assert.False(result[0].CanDelete);
    }

    [Fact]
    public async Task GetMessagesAsync_EventHasNoAdmin_RegularUserCannotDeleteOthersMessages()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        int eventId = 1;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var messages = new List<DiscussionMessage>
        {
            new DiscussionMessage(1, "not mine", DateTime.UtcNow) { Author = new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000099") } }
        };
        mockRepo.Setup(r => r.GetByEventAsync(eventId, userId)).ReturnsAsync(messages);

        var result = await service.GetMessagesAsync(eventId, userId);

        Assert.False(result[0].CanDelete);
    }

    [Fact]
    public async Task CreateMessageAsync_EmptyTextAndMedia_ThrowsArgumentException() {
        await Assert.ThrowsAsync<ArgumentException>(async () => await service.CreateMessageAsync("", "", 1, Guid.Parse("00000000-0000-0000-0000-000000000001"), null));
    }

    [Fact]
    public async Task CreateMessageAsync_LowReputation_ThrowsInvalidOperationException() {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        await service.CreateMessageAsync("Hi", null, 1, userId, null));
    }

    [Fact]
    public async Task CreateMessageAsync_EventNotFound_ThrowsArgumentException()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        mockEventRepo.Setup(e => e.GetByIdAsync(99)).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateMessageAsync("hello nonexistent event", null, 99, userId, null));
    }


    [Fact]
    public async Task CreateMessageAsync_UserIsMutedPermanently_ThrowsInvalidOperationException() {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 1;
        var evt = MakeEvent(eventId);
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var permanentMute = new DiscussionMute { IsPermanent = true };
        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync(permanentMute);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        await service.CreateMessageAsync("im permanently muted!", null, eventId, userId, null));
    }

    [Fact]
    public async Task CreateMessageAsync_UserIsMutedTemporarily_ThrowsInvalidOperationException() {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 1;
        var evt = MakeEvent(eventId);
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var temporaryMute = new DiscussionMute { MutedUntil = DateTime.UtcNow.AddHours(1), IsPermanent = false};
        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync(temporaryMute);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
        service.CreateMessageAsync("heyy", null, eventId, userId, null));
    }

    [Fact]
    public async Task CreateMessageAsync_SetsMediaPathOnPersistedMessage()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync((DiscussionMute?)null);
        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(new List<User>());

        DiscussionMessage? captured = null;
        mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>()))
                .Callback<DiscussionMessage>(m => captured = m)
                .ReturnsAsync(0);

        await service.CreateMessageAsync(null, "/tmp/photo.jpg", eventId, userId, null);

        Assert.Equal("/tmp/photo.jpg", captured!.MediaPath);
    }

    [Fact]
    public async Task CreateMessageAsync_MessageContainsTrailingWhitespaces_TrimsWhitespacesFromText()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync((DiscussionMute?)null);
        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(new List<User>());

        DiscussionMessage? captured = null;
        mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>()))
                .Callback<DiscussionMessage>(m => captured = m)
                .ReturnsAsync(0);

        await service.CreateMessageAsync("  hello  ", null, eventId, userId, null);

        Assert.Equal("hello", captured!.Message);
    }

    [Fact]
    public async Task CreateMessageAsync_MuteHasNoMutedUntilAndIsNotPermanent_MessageIsSent()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync(new DiscussionMute { IsPermanent = false, MutedUntil = null });
        mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>())).ReturnsAsync(0);
        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(new List<User>());

        await service.CreateMessageAsync("hello", null, eventId, userId, null);

        mockRepo.Verify(r => r.AddAsync(It.IsAny<DiscussionMessage>()), Times.Once);
    }

    [Fact]
    public async Task CreateMessageAsync_ExpiredMute_AutoUnmutesAndSucceeds()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId))
                .ReturnsAsync(new DiscussionMute
                {
                    IsPermanent = false,
                    MutedUntil = DateTime.UtcNow.AddSeconds(-1)
                });

        mockRepo.Setup(r => r.GetLastUserMessageDateAsync(eventId, userId))
               .ReturnsAsync((DateTime?)null);

        mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>())).ReturnsAsync(0);
        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(new List<User>());

        await service.CreateMessageAsync("im not muted anymore", null, eventId, userId, null);

        mockRepo.Verify(r => r.UnmuteAsync(eventId, userId), Times.Once);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<DiscussionMessage>()), Times.Once);
    }

    [Fact]
    public async Task CreateMessageAsync_RegularUserSendsMessageDuringSlowmode_ThrowsException() {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        var evt = MakeEvent(eventId,null,60);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetLastUserMessageDateAsync(eventId, userId)).ReturnsAsync(DateTime.UtcNow.AddSeconds(-10));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateMessageAsync("Heyooo", null, eventId, userId, null));
    }

    [Fact]
    public async Task CreateMessageAsync_UserSendsMessageAfterSlowModeExpired_MessageIsSent()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);

        var evt = MakeEvent(eventId, null, 30);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync((DiscussionMute?)null);

        mockRepo.Setup(r => r.GetLastUserMessageDateAsync(eventId, userId))
                .ReturnsAsync(DateTime.UtcNow.AddSeconds(-60)); 

        mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>())).ReturnsAsync(0);

        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(new List<User>());

        await service.CreateMessageAsync("Now I can post", null, eventId, userId, null);

        mockRepo.Verify(r => r.AddAsync(It.IsAny<DiscussionMessage>()), Times.Once);
    }

    [Fact]
    public async Task CreateMessageAsync_UserSendsMessageDuringSlowModeNoLastMessage_Succeeds()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);

        var evt = MakeEvent(eventId, null, 60);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync((DiscussionMute?)null);

        mockRepo.Setup(r => r.GetLastUserMessageDateAsync(eventId, userId))
                .ReturnsAsync((DateTime?)null);

        mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>())).ReturnsAsync(0);
        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(new List<User>());

        await service.CreateMessageAsync("first message", null, eventId, userId, null);

        mockRepo.Verify(r => r.AddAsync(It.IsAny<DiscussionMessage>()), Times.Once);
    }


    [Fact]
    public async Task CreateMessageAsync_AdminSendsMessageDuringSlowmode_AdminBypassesSlowmode()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(adminId)).ReturnsAsync(true);

        var evt = MakeEvent(eventId, adminId, 60);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId))
            .ReturnsAsync(evt);

        mockRepo.Setup(r => r.GetLastUserMessageDateAsync(eventId, adminId))
            .ReturnsAsync(DateTime.UtcNow.AddSeconds(-1));

        await service.CreateMessageAsync("take everything!", null, eventId, adminId, null);

        mockRepo.Verify(r => r.AddAsync(It.IsAny<DiscussionMessage>()), Times.Once);
    }

    [Fact]
    public async Task CreateMessageAsync_UserRepliesToMessage_SetsReplyOnMessage()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10, replyToId = 55;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);

        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId))
            .ReturnsAsync(evt);

        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync((DiscussionMute?)null);

        mockRepo.Setup(r => r.GetLastUserMessageDateAsync(eventId, userId))
                .ReturnsAsync((DateTime?)null);

        DiscussionMessage? captured = null;
        mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>()))
                .Callback<DiscussionMessage>(m => captured = m)
                .ReturnsAsync(0);

        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(new List<User>());

        await service.CreateMessageAsync("replying!", null, eventId, userId, replyToId);

        Assert.NotNull(captured?.ReplyTo);
    }

    [Fact]
    public async Task CreateMessageAsync_MessageMentionsUser_SendsNotification()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        Guid mentionedId = Guid.Parse("00000000-0000-0000-0000-000000000042");
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);

        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId))
            .ReturnsAsync(evt);

        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync((DiscussionMute?)null);

        mockRepo.Setup(r => r.GetLastUserMessageDateAsync(eventId, userId))
                .ReturnsAsync((DateTime?)null);

        mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>())).ReturnsAsync(0);

        var participants = new List<User>
        {
            new User { UserId = userId,      Name = "David" },
            new User { UserId = mentionedId, Name = "Anisia" }
        };
        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(participants);

        await service.CreateMessageAsync("Hey @Anisia thanks!", null, eventId, userId, null);

        mockNotification.Verify(
            n => n.NotifyAsync(mentionedId, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteMessageAsync_EventNotFound_ThrowsArgumentException()
    {
        mockEventRepo.Setup(e => e.GetByIdAsync(99)).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.DeleteMessageAsync(1, Guid.Parse("00000000-0000-0000-0000-000000000001"), 99));
    }

    [Fact]
    public async Task DeleteMessageAsync_AdminDeletesOthersMessage_MessageIsDeleted() {
        Guid authorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        int messageId = 2;
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var evt = MakeEvent(eventId, adminId);
        var mockMessage = new DiscussionMessage(messageId, "i am so oo done", DateTime.UtcNow) { Author = new User { UserId = authorId } };
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(mockMessage);

        await service.DeleteMessageAsync(messageId, authorId, eventId);

        mockRepo.Verify(r => r.DeleteAsync(messageId), Times.Once);
    }

    [Fact]
    public async Task DeleteMessageAsync_AdminDeletesNonexistentMessage_ThrowsException()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10;
        int messageId = 1011;
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync((DiscussionMessage)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        await service.DeleteMessageAsync(messageId, adminId, eventId));
    }

    [Fact]
    public async Task DeleteMessageAsync_NonAdminDeletesOthersMessage_ThrowsException() {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        int messageId = 101;
        Guid otherId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var evt = MakeEvent(eventId);
        var mockMessage = new DiscussionMessage(messageId, "sexyy_red_for_president", DateTime.UtcNow) { Author = new User { UserId = otherId } };
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(mockMessage);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        await service.DeleteMessageAsync(messageId, userId, eventId));

    }

    [Fact]
    public async Task DeleteMessageAsync_AuthorDeletesOwnMessage_Succeeds()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10, messageId = 50;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        var msg = new DiscussionMessage(messageId, "my msg", DateTime.UtcNow)
        { Author = new User { UserId = userId } };
        mockRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(msg);

        await service.DeleteMessageAsync(messageId, userId, eventId);

        mockRepo.Verify(r => r.DeleteAsync(messageId), Times.Once);
    }

    [Fact]
    public async Task DeleteMessageAsync_AdminDeletesOwnMessage_NoReputationPenalty()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10, messageId = 50;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        var msg = new DiscussionMessage(messageId, "admin's own", DateTime.UtcNow)
        { Author = new User { UserId = adminId } };
        mockRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(msg);

        await service.DeleteMessageAsync(messageId, adminId, eventId);

        mockRepo.Verify(r => r.DeleteAsync(messageId), Times.Once);
    }

    [Fact]
    public async Task DeleteMessageAsync_UserDeletesMessageWithNullAuthor_ThrowsUnauthorized()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10, messageId = 77;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        var msg = new DiscussionMessage(messageId, "ghost msg", DateTime.UtcNow) { Author = null };
        mockRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(msg);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.DeleteMessageAsync(messageId, userId, eventId));
    }

    [Fact]
    public async Task DeleteMessageAsync_AdminDeletesMessageWithNullAuthor_DeletesWithoutReputationPenalty()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10, messageId = 42;
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var msg = new DiscussionMessage(messageId, "nobody's messagee", DateTime.UtcNow) { Author = null };
        mockRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(msg);

        await service.DeleteMessageAsync(messageId, adminId, eventId);

        mockRepo.Verify(r => r.DeleteAsync(messageId), Times.Once);
    }

    [Fact]
    public async Task DeleteMessageAsync_AdminDeletesOthersMessage_SendsReputationMessage()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        Guid authorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        int messageId = 50;

        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var mockMessage = new DiscussionMessage(messageId, "somethingbad", DateTime.UtcNow)
        {
            Author = new User { UserId = authorId }
        };
        mockRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(mockMessage);

        await service.DeleteMessageAsync(messageId, adminId, eventId);

        mockRepo.Verify(r => r.DeleteAsync(messageId), Times.Once);


    }

    [Fact]
    public async Task RemoveReactionAsync_RemoveReactionFromAMessage_ReactionIsRemoved()
    {
        mockRepo.Setup(r => r.RemoveReactionAsync(1, Guid.Parse("00000000-0000-0000-0000-000000000002"))).Returns(Task.CompletedTask);

        await service.RemoveReactionAsync(1, Guid.Parse("00000000-0000-0000-0000-000000000002"));

        mockRepo.Verify(r => r.RemoveReactionAsync(1, Guid.Parse("00000000-0000-0000-0000-000000000002")), Times.Once);
    }

    [Fact]
    public async Task MuteUserAsync_NonAdminTriesToMute_ThrowsUnauthorizedException()
    {
        int userId = 1, eventId = 10;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.MuteUserAsync(eventId, 2, DateTime.UtcNow.AddHours(1), userId));
    }

    [Fact]
    public async Task MuteUserAsync_AdminMutesUserPermanently_MuteIsAppliedForGood()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10, targetId = 5;
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        DiscussionMute? captured = null;
        mockRepo.Setup(r => r.InsertMuteAsync(It.IsAny<DiscussionMute>()))
                .Callback<DiscussionMute>(m => captured = m)
                .Returns(Task.CompletedTask);

        await service.MuteUserAsync(eventId, targetId, null, adminId);

        Assert.True(captured!.IsPermanent);
    }

    [Fact]
    public async Task MuteUserAsync_AdminMutesUserTemporarily_MuteIsAppliedForAWhile()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10, targetId = 5;
        var until = DateTime.UtcNow.AddHours(2);
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        DiscussionMute? captured = null;
        mockRepo.Setup(r => r.InsertMuteAsync(It.IsAny<DiscussionMute>()))
                .Callback<DiscussionMute>(m => captured = m)
                .Returns(Task.CompletedTask);

        await service.MuteUserAsync(eventId, targetId, until, adminId);

        Assert.False(captured!.IsPermanent);
    }

    [Fact]
    public async Task UnmuteUserAsync_NonAdminAttemptsUnmute_ThrowsUnauthorizedException()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UnmuteUserAsync(eventId, Guid.Parse("00000000-0000-0000-0000-000000000002"), userId));
    }

    [Fact]
    public async Task UnmuteUserAsync_AdminUnmutesUser_UserIsUnmuted()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10;
        Guid targetId = Guid.Parse("00000000-0000-0000-0000-000000000005");
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.UnmuteAsync(eventId, targetId)).Returns(Task.CompletedTask);

        await service.UnmuteUserAsync(eventId, targetId, adminId);

        mockRepo.Verify(r => r.UnmuteAsync(eventId, targetId), Times.Once);
    }

    [Fact]
    public async Task GetEventParticipantsAsync_ReturnsList()
    {
        int eventId = 10;
        var participants = new List<User> { new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "davidpopescu" } };
        mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(participants);

        var result = await service.GetEventParticipantsAsync(eventId);

        Assert.Equal("davidpopescu", result[0].Name);
    }

    [Fact]
    public void FindMentionedUsers_FullNameMatch_ReturnsUser()
    {
        var users = new List<User> { new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "david popescu" } };

        var result = DiscussionService.FindMentionedUsers("Hey @david popescu!", users);

        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), result[0].UserId);
    }

    [Fact]
    public void FindMentionedUsers_FirstNameMatch_ReturnsUser()
    {
        var users = new List<User> { new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "david popescu" } };

        var result = DiscussionService.FindMentionedUsers("Hey @david check this", users);

        Assert.Single(result);
    }

    [Fact]
    public void FindMentionedUsers_NoMatchFound_ReturnsEmpty()
    {
        var users = new List<User> { new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "david" } };

        var result = DiscussionService.FindMentionedUsers("Hello @nobody", users);

        Assert.Empty(result);
    }

    [Fact]
    public void FindMentionedUsers_MentionIsNotExactMatch_FindsUserRegardless()
    {
        var users = new List<User> { new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "david" } };

        var result = DiscussionService.FindMentionedUsers("hi @DAVID", users);

        Assert.Single(result);
    }

    [Fact]
    public void FindMentionedUsers_MultipleMatchesFound_ReturnsAllMatches()
    {
        var users = new List<User>
        {
            new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "anisia" },
            new User { UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "david" }
        };

        var result = DiscussionService.FindMentionedUsers("@anisia and @david!", users);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SetSlowModeAsync_NonAdmin_ThrowsUnauthorizedAccessException()
    {
        int eventId = 10;
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        Guid maliciousUserId = Guid.Parse("00000000-0000-0000-0000-000000000666");

        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.SetSlowModeAsync(eventId, 30, maliciousUserId));
    }

    [Fact]
    public async Task SetSlowModeAsync_NegativeSeconds_ThrowsArgumentException()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10;
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SetSlowModeAsync(eventId, -5, adminId));
    }

    [Fact]
    public async Task SetSlowModeAsync_SlowmodeHasValidDuration_SlowmodeIsApplied()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10;
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.SetSlowModeAsync(eventId, 30)).Returns(Task.CompletedTask);

        await service.SetSlowModeAsync(eventId, 30, adminId);

        mockRepo.Verify(r => r.SetSlowModeAsync(eventId, 30), Times.Once);
    }

    [Fact]
    public async Task SetSlowModeAsync_SlowmodeHasNullSeconds_DisablesSlowmode()
    {
        Guid adminId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        int eventId = 10;
        var evt = MakeEvent(eventId, adminId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.SetSlowModeAsync(eventId, null)).Returns(Task.CompletedTask);


        await service.SetSlowModeAsync(eventId, null, adminId);

        mockRepo.Verify(r => r.SetSlowModeAsync(eventId, null), Times.Once);
    }

    [Fact]
    public async Task GetSlowModeSecondsAsync_ReturnsEventSlowMode()
    {
        int eventId = 10;
        var evt = MakeEvent(eventId, null, 45);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);

        var result = await service.GetSlowModeSecondsAsync(eventId);

        Assert.Equal(45, result);
    }

    [Fact]
    public async Task GetSlowModeSecondsAsync_EventNotFound_ThrowsArgumentException()
    {
        mockEventRepo.Setup(e => e.GetByIdAsync(99)).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetSlowModeSecondsAsync(99));
    }

    [Fact]
    public async Task CreateMessageAsync_MutedLessThanOneHour_ErrorDoesNotShowHoursOnlyMinutesAndSeconds()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);

        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId))
                .ReturnsAsync(new DiscussionMute
                {
                    IsPermanent = false,
                    MutedUntil = DateTime.UtcNow.AddMinutes(30) 
                });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateMessageAsync("hi", null, eventId, userId, null));

        Assert.DoesNotContain("h", ex.Message);
    }

    [Fact]
public async Task CreateMessageAsync_MentionerNotInParticipants_UsesSomeoneFallback()
{
    Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    int eventId = 10;
    Guid mentionedId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
    var evt = MakeEvent(eventId);
    mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
    mockRepo.Setup(r => r.GetMuteAsync(eventId, userId)).ReturnsAsync((DiscussionMute?)null);
    
    mockRepo.Setup(r => r.AddAsync(It.IsAny<DiscussionMessage>())).ReturnsAsync(0);

    var participants = new List<User> 
    { 
        new User { UserId = mentionedId, Name = "the target" } 
    };
    mockRepo.Setup(r => r.GetEventParticipantsAsync(eventId)).ReturnsAsync(participants);
        
    await service.CreateMessageAsync("hello @the target", null, eventId, userId, null);

    mockNotification.Verify(n => n.NotifyAsync(
        mentionedId, 
        It.IsAny<string>(), 
        It.Is<string>(s => s.Contains("Someone mentioned you"))), 
        Times.Once);
}

    [Fact]
    public async Task CreateMessageAsync_MutedMoreThanOneHour_ErrorShowsHoursAndMinutes()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        int eventId = 10;
        mockReputation.Setup(r => r.CanPostMessagesAsync(userId)).ReturnsAsync(true);
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);
        mockRepo.Setup(r => r.GetMuteAsync(eventId, userId))
                .ReturnsAsync(new DiscussionMute
                {
                    IsPermanent = false,
                    MutedUntil = DateTime.UtcNow.AddHours(2).AddMinutes(15) 
                });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateMessageAsync("hi", null, eventId, userId, null));

        Assert.Contains("h ", ex.Message);
    }

    [Fact]
    public async Task ReactAsync_UserReactsToMessageWithUniqueReaction_ReactionIsAddedToMessage()
    {
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        int messageId = 1;
        string emoji = "👍";

        mockRepo.Setup(r => r.GetReactionAsync(messageId, userId))
                .ReturnsAsync((DiscussionReaction?)null);
        mockRepo.Setup(r => r.AddReactionAsync(messageId, userId, emoji))
                .Returns(Task.CompletedTask);

        await service.ReactAsync(messageId, userId, emoji);

        mockRepo.Verify(r => r.AddReactionAsync(messageId, userId, emoji), Times.Once);
        mockRepo.Verify(r => r.UpdateReactionAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReactAsync_UserReactsToMessageWithExistingReaction_ReactionIsUpdated()
    {
        int messageId = 1;
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        string emoji = "👍";

        mockRepo.Setup(r => r.GetReactionAsync(messageId, userId))
                .ReturnsAsync(new DiscussionReaction
                {
                    Id = 1,
                    Emoji = "👍",
                    Message = new DiscussionMessage(messageId, null, DateTime.MinValue),
                    Author = new User { UserId = userId }

                });
        mockRepo.Setup(r => r.UpdateReactionAsync(messageId, userId, emoji))
                .Returns(Task.CompletedTask);

        await service.ReactAsync(messageId, userId, emoji);

        mockRepo.Verify(r => r.UpdateReactionAsync(messageId, userId, emoji), Times.Once);
    }


    [Fact]
    public async Task MuteUserAsync_EventNotFound_ThrowsArgumentException()
    {
        mockEventRepo.Setup(e => e.GetByIdAsync(99)).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.MuteUserAsync(99, 5, DateTime.UtcNow.AddHours(1), 1));
    }

    [Fact]
    public async Task UnmuteUserAsync_EventNotFound_ThrowsArgumentException()
    {
        mockEventRepo.Setup(e => e.GetByIdAsync(99)).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UnmuteUserAsync(99, Guid.Parse("00000000-0000-0000-0000-000000000005"), Guid.Parse("00000000-0000-0000-0000-000000000001")));
    }

    [Fact]
    public async Task SetSlowModeAsync_EventNotFound_ThrowsArgumentException()
    {
        mockEventRepo.Setup(e => e.GetByIdAsync(99)).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SetSlowModeAsync(99, 30, Guid.Parse("00000000-0000-0000-0000-000000000001")));
    }


    [Fact]
    public async Task MuteUserAsync_EventHasNoAdmin_ThrowsUnauthorized()
    {
        int eventId = 10;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);


        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.MuteUserAsync(eventId, 5, DateTime.UtcNow.AddHours(1), 1));
    }

    [Fact]
    public async Task UnmuteUserAsync_EventHasNoAdmin_ThrowsUnauthorized()
    {
        int eventId = 10;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);


        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UnmuteUserAsync(eventId, Guid.Parse("00000000-0000-0000-0000-000000000005"), Guid.Parse("00000000-0000-0000-0000-000000000001")));
    }

    [Fact]
    public async Task SetSlowModeAsync_EventHasNoAdmin_ThrowsUnauthorized()
    {
        int eventId = 10;
        var evt = MakeEvent(eventId);
        mockEventRepo.Setup(e => e.GetByIdAsync(eventId)).ReturnsAsync(evt);


        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.SetSlowModeAsync(eventId, 30, Guid.Parse("00000000-0000-0000-0000-000000000001")));
    }
    


    
}
