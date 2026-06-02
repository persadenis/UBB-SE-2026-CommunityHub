using ChatAndEvents.Data.EventsData.Services.discussionService;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.EventsData.ViewModelsCore;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class DiscussionController : Controller
{
    private readonly IDiscussionService _discussionService;
    private readonly IUserService _userService;

    public DiscussionController(IDiscussionService discussionService, IUserService userService)
    {
        _discussionService = discussionService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId, bool isAdmin = false)
    {
        var currentUser = await _userService.GetCurrentUser();
        var messages = await _discussionService.GetMessagesAsync(eventId, currentUser.UserId);
        var slowMode = await _discussionService.GetSlowModeSecondsAsync(eventId);

        var vm = new DiscussionViewModel
        {
            EventId = eventId,
            IsEventAdmin = isAdmin,
            CurrentSlowModeSeconds = slowMode,
            CurrentUserId = currentUser.UserId,
            Messages = messages.Select(m => new DiscussionMessageViewModel
            {
                Message = m,
                ReactionGroups = DiscussionMessageItemViewModelCore
                    .BuildReactionGroups(m.Reactions, currentUser.UserId),
                CurrentUserEmoji = DiscussionMessageItemViewModelCore
                    .CurrentUserEmoji(m.Reactions, currentUser.UserId),
                ShowMuteButton = DiscussionMessageItemViewModelCore
                    .ShowMuteButton(isAdmin, m.Author?.UserId, currentUser.UserId),
            }).ToList()
        };

        if (TempData["ErrorMessage"] is string err)
            vm.ErrorMessage = err;

        return View(vm);
    }

 

    [HttpPost]
    public async Task<IActionResult> Send(int eventId, string? newMessage, int? replyToId, bool isAdmin)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            await _discussionService.CreateMessageAsync(
                newMessage?.Trim(), null, eventId, currentUser.UserId, replyToId);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, isAdmin });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int messageId, int eventId, bool isAdmin)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            await _discussionService.DeleteMessageAsync(messageId, currentUser.UserId, eventId);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, isAdmin });
    }

    [HttpPost]
    public async Task<IActionResult> React(int messageId, int eventId, string emoji, bool isAdmin)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            await _discussionService.ReactAsync(messageId, currentUser.UserId, emoji);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, isAdmin });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveReaction(int messageId, int eventId, bool isAdmin)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            await _discussionService.RemoveReactionAsync(messageId, currentUser.UserId);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, isAdmin });
    }

    [HttpPost]
    public async Task<IActionResult> Mute(int eventId, Guid targetUserId,
  string duration, double customHours, double customMinutes, bool isAdmin)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            DateTime? muteUntil = DiscussionViewModelCore
                .CalculateMuteExpiry(duration, customHours, customMinutes, DateTime.UtcNow);
            await _discussionService.MuteUserAsync(
                eventId, targetUserId, muteUntil, currentUser.UserId);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, isAdmin });
    }
    [HttpPost]
    public async Task<IActionResult> Unmute(int eventId, Guid targetUserId, bool isAdmin)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            await _discussionService.UnmuteUserAsync(eventId, targetUserId, currentUser.UserId);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, isAdmin });
    }

    [HttpPost]
    public async Task<IActionResult> SetSlowMode(int eventId, int? seconds, bool isAdmin)
    {
        var currentUser = await _userService.GetCurrentUser();
        try
        {
            await _discussionService.SetSlowModeAsync(eventId, seconds, currentUser.UserId);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { eventId, isAdmin });
    }
}