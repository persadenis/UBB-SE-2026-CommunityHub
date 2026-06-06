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
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
    };

    private readonly ICommunityHubService _communityHubService;
    private readonly INotificationService _notificationService;
    private readonly CurrentUserContext _currentUserContext;
    private readonly IWebHostEnvironment _environment;

    public CommunitiesController(
        ICommunityHubService communityHubService,
        INotificationService notificationService,
        CurrentUserContext currentUserContext,
        IWebHostEnvironment environment)
    {
        _communityHubService = communityHubService;
        _notificationService = notificationService;
        _currentUserContext = currentUserContext;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? query, string? category)
    {
        return await Discover(query, category);
    }

    [HttpGet]
    public async Task<IActionResult> Discover(string? query, string? category)
    {
        var viewModel = new CommunityIndexViewModel
        {
            Query = query,
            Category = category,
            Categories = await _communityHubService.GetCategoriesAsync(),
            Communities = await _communityHubService.SearchAsync(_currentUserContext.UserId, query, category),
        };

        return View("Discover", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Start()
    {
        return View(new CommunityIndexViewModel
        {
            Categories = await _communityHubService.GetCategoriesAsync(),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string description, string category, IFormFile? bannerFile)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
        {
            TempData["CommunityMessage"] = "Name and description are required.";
            return RedirectToAction(nameof(Start));
        }

        var bannerUrl = await SaveBannerAsync(bannerFile);
        var community = await _communityHubService.CreateAsync(_currentUserContext.UserId, name, description, category, bannerUrl);
        await _notificationService.NotifyAsync(
            _currentUserContext.UserId,
            "Community created",
            $"You created {community.Name}.",
            "Community",
            "Communities",
            community.Id.ToString());

        return RedirectToAction(nameof(Details), new { id = community.Id });
    }

    private async Task<string?> SaveBannerAsync(IFormFile? bannerFile)
    {
        if (bannerFile == null || bannerFile.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(bannerFile.FileName);
        if (!AllowedImageExtensions.Contains(extension))
        {
            TempData["CommunityMessage"] = "Only JPG, PNG, and WebP community banners are supported.";
            return null;
        }

        var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "communities");
        Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(uploadFolder, fileName);

        await using var stream = System.IO.File.Create(path);
        await bannerFile.CopyToAsync(stream);

        return $"/uploads/communities/{fileName}";
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
