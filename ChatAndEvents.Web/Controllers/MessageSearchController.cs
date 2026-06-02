using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.services;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class MessageSearchController : Controller
{
    private readonly ISearchService _searchService;
    private readonly CurrentUserContext _currentUserContext;

    public MessageSearchController(ISearchService searchService, CurrentUserContext currentUserContext)
    {
        _searchService = searchService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid conversationId, string query = "")
    {
        if (conversationId == Guid.Empty)
        {
            return BadRequest("A conversation id is required.");
        }

        var viewModel = new MessageSearchViewModel
        {
            ConversationId = conversationId,
            CurrentUserId = _currentUserContext.UserId,
            Query = query ?? string.Empty,
            ErrorMessage = TempData["MessageSearchError"] as string,
        };

        if (string.IsNullOrWhiteSpace(query))
        {
            viewModel.NoResultsMessage = "Enter text to search.";
            return View(viewModel);
        }

        viewModel.HasSearched = true;

        try
        {
            viewModel.Results = await _searchService.SearchMessagesAsync(
                conversationId,
                _currentUserContext.UserId,
                query.Trim());

            if (viewModel.Results.Count == 0)
            {
                viewModel.NoResultsMessage = "No messages found.";
            }
        }
        catch (Exception exception)
        {
            viewModel.ErrorMessage = exception.Message;
        }

        return View(viewModel);
    }
}
