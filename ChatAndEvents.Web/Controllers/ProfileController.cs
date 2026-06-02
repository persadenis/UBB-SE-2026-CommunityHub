using System;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.CommunityHub.Services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IProfileService _profileService;
    private readonly IFriendRequestService _friendRequestService;
    private readonly IBlockService _blockService;
    private readonly IDirectMessageService _directMessageService;
    private readonly IMatchmakingService _matchmakingService;
    private readonly CurrentUserContext _currentUserContext;

    public ProfileController(
        IProfileService profileService,
        IFriendRequestService friendRequestService,
        IBlockService blockService,
        IDirectMessageService directMessageService,
        IMatchmakingService matchmakingService,
        CurrentUserContext currentUserContext)
    {
        _profileService = profileService;
        _friendRequestService = friendRequestService;
        _blockService = blockService;
        _directMessageService = directMessageService;
        _matchmakingService = matchmakingService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [Route("Profile")]
    [Route("ProfileView")]
    public async Task<IActionResult> Index(Guid? userId)
    {
        var targetUserId = userId ?? _currentUserContext.UserId;
        var user = await _profileService.GetProfileAsync(targetUserId);

        if (user == null)
        {
            return NotFound();
        }

        var isOwnProfile = targetUserId == _currentUserContext.UserId;
        var isBlocked = false;
        FriendStatus? relationshipStatus = null;
        var mutualFriends = Enumerable.Empty<FriendListItemViewModel>().ToList();

        if (!isOwnProfile)
        {
            isBlocked = await _blockService.IsBlockedAsync(_currentUserContext.UserId, targetUserId);
            relationshipStatus = await _friendRequestService.GetRelationshipStatusAsync(
                _currentUserContext.UserId,
                targetUserId);
            var mutualUsers = await _profileService.GetMutualFriendsAsync(_currentUserContext.UserId, targetUserId);
            mutualFriends = mutualUsers.Select(mutualFriend => new FriendListItemViewModel(mutualFriend)).ToList();
        }

        var viewModel = ProfileViewModel.FromUser(
            user,
            isOwnProfile,
            isBlocked,
            relationshipStatus,
            mutualFriends,
            TempData["ProfileActionMessage"] as string);

        if (isOwnProfile)
        {
            var datingProfile = await _matchmakingService.GetProfileAsync(_currentUserContext.UserId);
            viewModel.HasMatchmakingProfile = datingProfile != null;
            viewModel.IsMatchmakingEnabled = datingProfile?.IsEnabled == true && datingProfile.IsArchived == false;
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string? bio, string? avatarUrl, DateTime? birthday, UserStatus status)
    {
        await _profileService.UpdateProfileAsync(_currentUserContext.UserId, bio, avatarUrl, birthday);
        await _profileService.UpdateStatusAsync(_currentUserContext.UserId, status);
        TempData["ProfileActionMessage"] = "Profile saved.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendFriendRequest(Guid userId)
    {
        if (userId != Guid.Empty)
        {
            await _friendRequestService.SendFriendRequestAsync(_currentUserContext.UserId, userId);
            TempData["ProfileActionMessage"] = "Friend request sent.";
        }

        return RedirectToAction(nameof(Index), new { userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(Guid userId)
    {
        if (userId != Guid.Empty)
        {
            await _blockService.BlockUserAsync(_currentUserContext.UserId, userId);
            TempData["ProfileActionMessage"] = "User blocked.";
        }

        return RedirectToAction(nameof(Index), new { userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(Guid userId)
    {
        if (userId != Guid.Empty)
        {
            await _blockService.UnblockUserAsync(_currentUserContext.UserId, userId);
            TempData["ProfileActionMessage"] = "User unblocked.";
        }

        return RedirectToAction(nameof(Index), new { userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenDirectMessage(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return RedirectToAction(nameof(Index));
        }

        var conversation = await _directMessageService.GetOrCreateAsync(_currentUserContext.UserId, userId);

        return RedirectToAction(
            "Index",
            "Chat",
            new { conversationId = conversation.Id, currentUserId = _currentUserContext.UserId });
    }
}
