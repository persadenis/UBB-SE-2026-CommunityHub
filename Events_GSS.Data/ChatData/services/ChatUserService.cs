using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories.reputationRepository;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.ChatData.services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChatUserService : IUserService
{
    private readonly IUserRepository _chatUserRepo;
    private readonly IReputationRepository _reputationRepo;
    private readonly IAttendedEventService _attendedEventService;
    private readonly IDbContextFactory<AppDbContext> _dbFactory; 
    private Guid currentUserId;

    public ChatUserService(
        IUserRepository chatUserRepo,
        IReputationRepository reputationRepo,
        IAttendedEventService attendedEventService,
        IDbContextFactory<AppDbContext> dbFactory) 
    {
        this._chatUserRepo = chatUserRepo;
        this._reputationRepo = reputationRepo;
        this._attendedEventService = attendedEventService;
        _dbFactory = dbFactory;
    }

    public void SetCurrentUserId(Guid userId)
    {
        currentUserId = userId;
    }

    public async Task<ChatAndEvents.Data.EventsData.Models.User> GetCurrentUser()
    {
        var reputationScore = await _reputationRepo.GetReputationScoreAsync(currentUserId);

        await using var db = await _dbFactory.CreateDbContextAsync(); 
        var eventsUser = await db.Set<ChatAndEvents.Data.EventsData.Models.User>()
            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        if (eventsUser == null) throw new Exception("User not found");

        eventsUser.ReputationPoints = reputationScore?.ReputationPoints ?? 0;
        eventsUser.ReputationScore = reputationScore;

        return eventsUser;
    }

    public async Task<ChatAndEvents.Data.EventsData.Models.User?> GetUserById(Guid userId)
    {
        var reputationScore = await _reputationRepo.GetReputationScoreAsync(userId);

        await using var db = await _dbFactory.CreateDbContextAsync(); 
        var eventsUser = await db.Set<ChatAndEvents.Data.EventsData.Models.User>()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (eventsUser == null) return null;

        eventsUser.ReputationPoints = reputationScore?.ReputationPoints ?? 0;
        eventsUser.ReputationScore = reputationScore;

        return eventsUser;
    }

    public List<ChatAndEvents.Data.EventsData.Models.User> GetFriends(Guid userId) => new();
    public List<ChatAndEvents.Data.EventsData.Models.User> SearchFriends(Guid userId, string name) => new();

    public async Task<bool> IsAttending(Event currentEvent)
    {
        var attendingEvents = await _attendedEventService.GetAttendedEventsAsync(currentUserId);
        return attendingEvents.Any(ae => ae.Event.EventId == currentEvent.EventId);
    }

    public bool IsAdmin(Event currentEvent)
    {
        return currentEvent.Admin.UserId == currentUserId;
    }
}