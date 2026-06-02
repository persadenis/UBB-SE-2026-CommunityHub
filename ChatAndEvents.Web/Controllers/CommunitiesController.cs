using ChatAndEvents.Data.CommunityHub.Services;
using ChatAndEvents.Data.EventsData.Services.notificationServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class CommunitiesController : Controller
{
    private readonly ICommunityHubService _communityHubService;
    private readonly INotificationService _notificationService;
    private readonly CurrentUserContext _currentUserContext;

    public CommunitiesController(
        ICommunityHubService communityHubService,
        INotificationService notificationService,
        CurrentUserContext currentUserContext)
    {
        _communityHubService = communityHubService;
        _notificationService = notificationService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? query, string? category)
    {
        var viewModel = new CommunityIndexViewModel
        {
            Query = query,
            Category = category,
            Categories = await _communityHubService.GetCategoriesAsync(),
            Communities = await _communityHubService.SearchAsync(_currentUserContext.UserId, query, category),
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
        {
            TempData["CommunityMessage"] = "Name and description are required.";
            return RedirectToAction(nameof(Index));
        }

        var community = await _communityHubService.CreateAsync(_currentUserContext.UserId, name, description, category);
        await _notificationService.NotifyAsync(
            _currentUserContext.UserId,
            "Community created",
            $"You created {community.Name}.",
            "Community",
            "Communities",
            community.Id.ToString());

        return RedirectToAction(nameof(Details), new { id = community.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var community = await _communityHubService.GetCommunityAsync(id, _currentUserContext.UserId);
        if (community == null)
        {
            return NotFound();
        }

        var membership = community.Members.FirstOrDefault(member => member.UserId == _currentUserContext.UserId);
        return View(new CommunityDetailsViewModel
        {
            Community = community,
            IsCurrentUserMember = membership != null,
            IsCurrentUserAdmin = membership?.IsAdmin == true,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(Guid id)
    {
        await _communityHubService.JoinAsync(id, _currentUserContext.UserId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(Guid id)
    {
        await _communityHubService.LeaveAsync(id, _currentUserContext.UserId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPost(Guid id, string title, string body)
    {
        try
        {
            await _communityHubService.AddPostAsync(id, _currentUserContext.UserId, title, body);
        }
        catch (Exception ex)
        {
            TempData["CommunityMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
