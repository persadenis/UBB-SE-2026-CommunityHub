using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class FriendListController : Controller
{
    private readonly IFriendListService _friendListService;
    private readonly IFriendRequestService _friendRequestService;
    private readonly IDirectMessageService _directMessageService;
    private readonly CurrentUserContext _currentUserContext;

    public FriendListController(
        IFriendListService friendListService,
        IFriendRequestService friendRequestService,
        IDirectMessageService directMessageService,
        CurrentUserContext currentUserContext)
    {
        _friendListService = friendListService;
        _friendRequestService = friendRequestService;
        _directMessageService = directMessageService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [Route("FriendList")]
    [Route("FriendListView")]
    public async Task<IActionResult> Index()
    {
        var friends = await _friendListService.GetFriendsAsync(_currentUserContext.UserId);

        var viewModel = new FriendListViewModel
        {
            Friends = friends.Select(friend => new FriendListItemViewModel(friend)).ToList(),
            FriendActionMessage = TempData["FriendActionMessage"] as string,
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendFriendRequest(string friendUsernameInput)
    {
        if (string.IsNullOrWhiteSpace(friendUsernameInput))
        {
            TempData["FriendActionMessage"] = "Enter a username first.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var isRequestSent = await _friendRequestService.SendFriendRequestByUsernameAsync(
                _currentUserContext.UserId,
                friendUsernameInput);

            TempData["FriendActionMessage"] = isRequestSent
                ? "Friend request sent."
                : "User not found.";
        }
        catch (HttpRequestException exception)
        {
            TempData["FriendActionMessage"] = exception.Message;
        }
        catch (InvalidOperationException exception)
        {
            TempData["FriendActionMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid friendId)
    {
        if (friendId != Guid.Empty)
        {
            await _friendListService.RemoveFriendAsync(_currentUserContext.UserId, friendId);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenDirectMessage(Guid friendId)
    {
        if (friendId == Guid.Empty)
        {
            return RedirectToAction(nameof(Index));
        }

        var conversation = await _directMessageService.GetOrCreateAsync(_currentUserContext.UserId, friendId);

        return RedirectToAction(
            "Index",
            "Chat",
            new { conversationId = conversation.Id, currentUserId = _currentUserContext.UserId });
    }
}
