using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
namespace ChatAndEvents.Web.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
        };

        private readonly IGroupService _groupService;
        private readonly ISearchService _searchService;
        private readonly CurrentUserContext _currentUserContext;
        private readonly IWebHostEnvironment _environment;

        public GroupController(
            IGroupService groupService,
            ISearchService searchService,
            CurrentUserContext currentUserContext,
            IWebHostEnvironment environment)
        {
            _groupService = groupService;
            _searchService = searchService;
            _currentUserContext = currentUserContext;
            _environment = environment;
        }

        private Guid GetCurrentUserId() => _currentUserContext.UserId;

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateGroupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGroupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var currentUserId = GetCurrentUserId();
                var uploadedIconUrl = await SaveGroupIconAsync(model.IconFile);
                if (!string.IsNullOrWhiteSpace(uploadedIconUrl))
                {
                    model.IconUrl = uploadedIconUrl;
                }

                var memberIds = string.IsNullOrWhiteSpace(model.SelectedMemberIds)
                    ? new List<Guid>()
                    : model.SelectedMemberIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => Guid.Parse(id.Trim()))
                        .ToList();

                await _groupService.CreateGroupAsync(
                    currentUserId,
                    model.GroupName,
                    model.IconUrl,
                    memberIds);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;

                // Re-populate selected members for display
                model.SelectedMembers = await ResolveSelectedMembersAsync(model.SelectedMemberIds);
                return View(model);
            }
        }

        private async Task<string?> SaveGroupIconAsync(IFormFile? iconFile)
        {
            if (iconFile == null || iconFile.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(iconFile.FileName);
            if (!AllowedImageExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only JPG, PNG, and WebP group icons are supported.");
            }

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "groups");
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var path = Path.Combine(uploadFolder, fileName);

            await using var stream = System.IO.File.Create(path);
            await iconFile.CopyToAsync(stream);

            return $"/uploads/groups/{fileName}";
        }

        // GET: /Group/SearchUsers?query=alice
        // Called by fetch() in the view — returns JSON
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<UserDto>());

            var currentUserId = GetCurrentUserId();
            var users = await _searchService.SearchUsersAsync(query);

            var results = users
                .Where(u => u.Id != currentUserId)
                .Select(u => new UserDto { Id = u.Id, Username = u.Username })
                .ToList();

            return Json(results);
        }

        private async Task<List<UserDto>> ResolveSelectedMembersAsync(string selectedMemberIds)
        {
            if (string.IsNullOrWhiteSpace(selectedMemberIds))
                return new List<UserDto>();

            var ids = selectedMemberIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.Parse(id.Trim()))
                .ToList();

            var allUsers = await _searchService.SearchUsersAsync(string.Empty);
            return allUsers
                .Where(u => ids.Contains(u.Id))
                .Select(u => new UserDto { Id = u.Id, Username = u.Username })
                .ToList();
        }
    }
}
