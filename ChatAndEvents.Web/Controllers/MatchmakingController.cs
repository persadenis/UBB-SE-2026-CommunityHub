using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.CommunityHub.Services;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class MatchmakingController : Controller
{
    private static readonly HashSet<string> AllowedPhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
    };

    private readonly IMatchmakingService _matchmakingService;
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly IDirectMessageService _directMessageService;
    private readonly CurrentUserContext _currentUserContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MatchmakingController(
        IMatchmakingService matchmakingService,
        INotificationService notificationService,
        IUserService userService,
        IDirectMessageService directMessageService,
        CurrentUserContext currentUserContext,
        IWebHostEnvironment environment,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        _matchmakingService = matchmakingService;
        _notificationService = notificationService;
        _userService = userService;
        _directMessageService = directMessageService;
        _currentUserContext = currentUserContext;
        _environment = environment;
        _contextFactory = contextFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profile = await _matchmakingService.GetProfileAsync(_currentUserContext.UserId);
        var candidates = profile?.IsEnabled == true
            ? await _matchmakingService.GetCandidatesAsync(_currentUserContext.UserId)
            : [];
        var matches = profile?.IsEnabled == true
            ? await _matchmakingService.GetMatchesAsync(_currentUserContext.UserId)
            : [];
        var friendUserIds = await GetAcceptedFriendIdsAsync(_currentUserContext.UserId);

        return View(new MatchmakingIndexViewModel
        {
            Profile = profile,
            Candidates = candidates,
            Matches = matches,
            FriendUserIds = friendUserIds,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var profile = await _matchmakingService.GetProfileAsync(_currentUserContext.UserId);
        var currentUser = await _userService.GetCurrentUser();
        return View(MatchmakingEditViewModel.FromProfile(profile, currentUser.Name));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MatchmakingEditViewModel model)
    {
        var uploadedPhotoUrls = await SaveUploadedPhotosAsync(model.PhotoFiles);
        var photoUrls = BuildPhotoList(model.ExistingPhotoUrls, uploadedPhotoUrls);
        var preferredGenders = model.PreferredGenderSelections.Any()
            ? string.Join(", ", model.PreferredGenderSelections)
            : model.PreferredGenders;

        await _matchmakingService.SaveProfileAsync(
            _currentUserContext.UserId,
            model.DisplayName,
            model.Gender,
            preferredGenders,
            model.Location,
            model.DatingBio,
            model.Interests,
            photoUrls,
            model.MinPreferredAge,
            model.MaxPreferredAge,
            model.MaxDistanceKm,
            model.LoverType,
            model.IsEnabled);

        await _notificationService.NotifyAsync(
            _currentUserContext.UserId,
            "Matchmaking profile updated",
            model.IsEnabled ? "Your matchmaking profile is active." : "Your matchmaking profile is hidden.",
            "Matchmaking",
            "Matchmaking",
            _currentUserContext.UserId.ToString());

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable()
    {
        await _matchmakingService.DisableAsync(_currentUserContext.UserId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> React(Guid targetUserId, string action)
    {
        if (targetUserId == Guid.Empty)
        {
            return RedirectToAction(nameof(Index));
        }

        var targetProfile = await _matchmakingService.GetProfileAsync(targetUserId);
        var isMatch = await _matchmakingService.ReactAsync(_currentUserContext.UserId, targetUserId, action);

        if (isMatch)
        {
            var targetName = targetProfile?.DisplayName ?? "this person";
            TempData["MatchmakingMessage"] = $"It's a match with {targetName}.";

            await _notificationService.NotifyAsync(
                _currentUserContext.UserId,
                "New matchmaking match",
                $"You matched with {targetName}.",
                "Matchmaking",
                "Matchmaking",
                targetUserId.ToString());

            await _notificationService.NotifyAsync(
                targetUserId,
                "New matchmaking match",
                "Someone you liked matched with you.",
                "Matchmaking",
                "Matchmaking",
                _currentUserContext.UserId.ToString());
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMatchedFriend(Guid targetUserId)
    {
        if (targetUserId == Guid.Empty)
        {
            return RedirectToAction(nameof(Index));
        }

        var targetProfile = await EnsureMatchedFriendshipAsync(targetUserId);
        TempData["MatchmakingMessage"] = $"{targetProfile.DisplayName} is now in your friends list.";

        await _notificationService.NotifyAsync(
            targetUserId,
            "Match added as friend",
            "A matchmaking match added you as a friend.",
            "FriendRequest",
            "Matchmaking",
            _currentUserContext.UserId.ToString());

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenMatchChat(Guid targetUserId)
    {
        if (targetUserId == Guid.Empty)
        {
            return RedirectToAction(nameof(Index));
        }

        await EnsureMatchedFriendshipAsync(targetUserId);
        var conversation = await _directMessageService.GetOrCreateAsync(_currentUserContext.UserId, targetUserId);

        return RedirectToAction(
            "Index",
            "Chat",
            new { conversationId = conversation.Id, currentUserId = _currentUserContext.UserId });
    }

    private async Task<IReadOnlyList<string>> SaveUploadedPhotosAsync(IEnumerable<IFormFile> photoFiles)
    {
        var savedUrls = new List<string>();
        var files = photoFiles
            .Where(file => file.Length > 0)
            .Take(6)
            .ToList();

        if (!files.Any())
        {
            return savedUrls;
        }

        var uploadFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "matchmaking",
            _currentUserContext.UserId.ToString("N"));

        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedPhotoExtensions.Contains(extension))
            {
                continue;
            }

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var path = Path.Combine(uploadFolder, fileName);
            await using var stream = System.IO.File.Create(path);
            await file.CopyToAsync(stream);

            savedUrls.Add($"/uploads/matchmaking/{_currentUserContext.UserId:N}/{fileName}");
        }

        return savedUrls;
    }

    private static string BuildPhotoList(string existingPhotoUrls, IReadOnlyList<string> uploadedPhotoUrls)
    {
        var existingUrls = (existingPhotoUrls ?? string.Empty)
            .Split(['\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return string.Join(Environment.NewLine, existingUrls.Concat(uploadedPhotoUrls).Distinct());
    }

    private async Task<ISet<Guid>> GetAcceptedFriendIdsAsync(Guid userId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var friendships = await db.Friends
            .Where(friend => friend.Status == FriendStatus.Accepted
                && (friend.UserId1 == userId || friend.UserId2 == userId))
            .ToListAsync();

        return friendships
            .Select(friend => friend.UserId1 == userId ? friend.UserId2 : friend.UserId1)
            .ToHashSet();
    }

    private async Task<ChatAndEvents.Data.CommunityHub.Models.DatingProfile> EnsureMatchedFriendshipAsync(Guid targetUserId)
    {
        var matches = await _matchmakingService.GetMatchesAsync(_currentUserContext.UserId);
        var targetProfile = matches
            .Select(match => match.Profile)
            .FirstOrDefault(profile => profile.UserId == targetUserId);

        if (targetProfile == null)
        {
            throw new InvalidOperationException("You can only add or chat with mutual matchmaking matches.");
        }

        await using var db = await _contextFactory.CreateDbContextAsync();
        var friendship = await db.Friends.FirstOrDefaultAsync(friend =>
            (friend.UserId1 == _currentUserContext.UserId && friend.UserId2 == targetUserId)
            || (friend.UserId1 == targetUserId && friend.UserId2 == _currentUserContext.UserId));

        if (friendship == null)
        {
            db.Friends.Add(new Friend
            {
                Id = Guid.NewGuid(),
                UserId1 = _currentUserContext.UserId,
                UserId2 = targetUserId,
                Status = FriendStatus.Accepted,
                IsMatch = true,
                CreatedAt = DateTime.UtcNow,
            });
        }
        else
        {
            friendship.Status = FriendStatus.Accepted;
            friendship.IsMatch = true;
        }

        await db.SaveChangesAsync();
        return targetProfile;
    }
}
