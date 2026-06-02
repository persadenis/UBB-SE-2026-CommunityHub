using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChatAndEvents.Web.Controllers
{
    [Authorize]
    public class ConversationListController : Controller
    {
        private readonly IConversationListService _conversationListService;
        private readonly IDirectMessageService _directMessageService;
        private readonly ISearchService _searchService;
        private readonly CurrentUserContext _currentUserContext;

        public ConversationListController(IConversationListService conversationListService, IDirectMessageService directMessageService, ISearchService searchService, CurrentUserContext currentUserContext)
        {
            _conversationListService = conversationListService;
            _directMessageService = directMessageService;
            _searchService = searchService;
            _currentUserContext = currentUserContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string activeTab = "All", string searchQuery = "")
        {
            var viewModel = new ConversationListViewModel
            {
                ActiveTab = activeTab,
                SearchQuery = searchQuery
            };

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                viewModel.Conversations = await _conversationListService.SearchAsync(_currentUserContext.UserId, searchQuery);
            }
            else
            {
                viewModel.Conversations = activeTab switch
                {
                    ConversationListViewModel.AllTab => await _conversationListService.GetAllAsync(_currentUserContext.UserId),
                    ConversationListViewModel.DirectMessagesTab => await _conversationListService.GetDmsAsync(_currentUserContext.UserId),
                    ConversationListViewModel.GroupsTab => await _conversationListService.GetGroupsAsync(_currentUserContext.UserId),
                    ConversationListViewModel.FavoritesTab => await _conversationListService.GetFavouritesAsync(_currentUserContext.UserId),
                    ConversationListViewModel.UnreadTab => await _conversationListService.GetUnreadAsync(_currentUserContext.UserId),
                    _ => await _conversationListService.GetAllAsync(_currentUserContext.UserId),
                };
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavourite(Guid conversationId, string currentTab, string currentSearch)
        {
            var favourites = await _conversationListService.GetFavouritesAsync(_currentUserContext.UserId);
            bool isFavourite = favourites.Exists(c => c.Id == conversationId);
            
            await _conversationListService.SetFavouriteAsync(conversationId, _currentUserContext.UserId, !isFavourite);
            
            return RedirectToAction("Index", new { activeTab = currentTab, searchQuery = currentSearch });
        }

        [HttpGet]
        public IActionResult CreateDm()
        {
            return View(new CreateDmViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDm(CreateDmViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _directMessageService.GetOrCreateAsync(
                    _currentUserContext.UserId,
                    model.TargetUserId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var currentUserId = _currentUserContext.UserId;
            var users = await _searchService.SearchUsersAsync(query);
            var results = users
                .Where(u => u.Id != currentUserId)
                .Select(u => new { id = u.Id, username = u.Username })
                .ToList();

            return Json(results);
        }
    }
}