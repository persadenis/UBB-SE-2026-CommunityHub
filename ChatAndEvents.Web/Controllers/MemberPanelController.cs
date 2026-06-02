using System;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class MemberPanelController : Controller
{
    private static readonly TimeSpan DefaultTimeoutDuration = TimeSpan.FromMinutes(10);

    private readonly IMemberPanelService _memberPanelService;
    private readonly IModerationService _moderationService;
    private readonly CurrentUserContext _currentUserContext;

    public MemberPanelController(
        IMemberPanelService memberPanelService,
        IModerationService moderationService,
        CurrentUserContext currentUserContext)
    {
        _memberPanelService = memberPanelService;
        _moderationService = moderationService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid conversationId, string addMemberQuery = "")
    {
        if (conversationId == Guid.Empty)
        {
            return BadRequest("A conversation id is required.");
        }

        var viewModel = await BuildViewModelAsync(conversationId, addMemberQuery);
        viewModel.ErrorMessage = TempData["MemberPanelError"] as string;
        viewModel.SuccessMessage = TempData["MemberPanelMessage"] as string;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(Guid conversationId, Guid newUserId, string addMemberQuery = "")
    {
        if (newUserId == Guid.Empty)
        {
            TempData["MemberPanelError"] = "Select a user to add.";
            return RedirectToAction(nameof(Index), new { conversationId, addMemberQuery });
        }

        await RunModerationActionAsync(
            () => _moderationService.AddMemberAsync(conversationId, _currentUserContext.UserId, newUserId),
            "Member added.");

        return RedirectToAction(nameof(Index), new { conversationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ban(Guid conversationId, Guid userId)
    {
        await RunModerationActionAsync(
            () => _moderationService.BanMemberAsync(conversationId, _currentUserContext.UserId, userId),
            "Member banned.");

        return RedirectToAction(nameof(Index), new { conversationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unban(Guid conversationId, Guid userId)
    {
        await RunModerationActionAsync(
            () => _moderationService.UnbanMemberAsync(conversationId, _currentUserContext.UserId, userId),
            "Member unbanned.");

        return RedirectToAction(nameof(Index), new { conversationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Timeout(Guid conversationId, Guid userId, int durationMinutes = 10)
    {
        var duration = durationMinutes > 0 ? TimeSpan.FromMinutes(durationMinutes) : DefaultTimeoutDuration;

        await RunModerationActionAsync(
            () => _moderationService.TimeoutMemberAsync(conversationId, _currentUserContext.UserId, userId, duration),
            "Member timed out.");

        return RedirectToAction(nameof(Index), new { conversationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTimeout(Guid conversationId, Guid userId)
    {
        await RunModerationActionAsync(
            () => _moderationService.RemoveTimeoutAsync(conversationId, _currentUserContext.UserId, userId),
            "Timeout removed.");

        return RedirectToAction(nameof(Index), new { conversationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Promote(Guid conversationId, Guid userId)
    {
        await RunModerationActionAsync(
            () => _moderationService.PromoteMemberAsync(conversationId, _currentUserContext.UserId, userId),
            "Member promoted.");

        return RedirectToAction(nameof(Index), new { conversationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Demote(Guid conversationId, Guid userId)
    {
        await RunModerationActionAsync(
            () => _moderationService.DemoteMemberAsync(conversationId, _currentUserContext.UserId, userId),
            "Member demoted.");

        return RedirectToAction(nameof(Index), new { conversationId });
    }

    private async Task<MemberPanelViewModel> BuildViewModelAsync(Guid conversationId, string addMemberQuery)
    {
        var participants = await _memberPanelService.GetMembersAsync(conversationId);
        var members = new MemberPanelViewModel
        {
            ConversationId = conversationId,
            CurrentUserId = _currentUserContext.UserId,
            AddMemberQuery = addMemberQuery ?? string.Empty,
        };

        foreach (var participant in participants)
        {
            var user = await _memberPanelService.GetUserAsync(participant.UserId);
            if (user == null)
            {
                continue;
            }

            var item = ToDisplayItem(participant, user);
            if (participant.Role == ParticipantRole.Banned)
            {
                members.BannedMembers.Add(item);
            }
            else
            {
                members.Members.Add(item);
            }
        }

        members.IsAdmin = participants.Any(participant =>
            participant.UserId == _currentUserContext.UserId &&
            participant.Role == ParticipantRole.Admin);

        if (members.IsAdmin && !string.IsNullOrWhiteSpace(addMemberQuery))
        {
            members.AddMemberResults = await _memberPanelService.SearchUsersToAddAsync(conversationId, addMemberQuery);
        }

        return members;
    }

    private static MemberDisplayItem ToDisplayItem(Participant participant, User user)
    {
        return new MemberDisplayItem
        {
            UserId = participant.UserId,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            Role = participant.Role,
            HasTimeout = participant.TimeoutUntil.HasValue && participant.TimeoutUntil.Value > DateTime.UtcNow,
            TimeoutUntil = participant.TimeoutUntil,
        };
    }

    private async Task RunModerationActionAsync(Func<Task> action, string successMessage)
    {
        try
        {
            await action();
            TempData["MemberPanelMessage"] = successMessage;
        }
        catch (Exception exception)
        {
            TempData["MemberPanelError"] = exception.Message;
        }
    }
}
