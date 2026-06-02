using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly ISearchService _searchService;
        private readonly CurrentUserContext _currentUserContext;

        public GroupController(
            IGroupService groupService,
            ISearchService searchService,
            CurrentUserContext currentUserContext)
        {
            _groupService = groupService;
            _searchService = searchService;
            _currentUserContext = currentUserContext;
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