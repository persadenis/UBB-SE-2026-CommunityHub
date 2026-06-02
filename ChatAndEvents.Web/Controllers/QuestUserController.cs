using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.EventsData.ViewModelsCore;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class QuestUserController : Controller
{
    private readonly IEventService _eventService;
    private readonly IQuestApprovalService _questApprovalService;
    private readonly IUserService _userService;

    public QuestUserController(
        IEventService eventService,
        IQuestApprovalService questApprovalService,
        IUserService userService)
    {
        _eventService = eventService;
        _questApprovalService = questApprovalService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId, string filter = QuestUserFilters.All)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound();
        }

        var viewModel = await BuildViewModelAsync(currentEvent, filter);

        if (TempData["QuestUserError"] is string errorMessage)
        {
            viewModel.ErrorMessage = errorMessage;
        }

        if (TempData["QuestUserSuccess"] is string successMessage)
        {
            viewModel.SuccessMessage = successMessage;
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitProof(
        int eventId,
        int questId,
        string filter,
        string? photoPath,
        string? text)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound();
        }

        try
        {
            var currentUser = await _userService.GetCurrentUser();
            var questMemory = await FindQuestMemoryAsync(currentEvent, currentUser, questId);
            if (questMemory == null)
            {
                TempData["QuestUserError"] = "Quest could not be found.";
                return RedirectToAction(nameof(Index), new { eventId, filter });
            }

            var proof = new Memory(photoPath, text, DateTime.UtcNow)
            {
                Event = currentEvent,
                Author = currentUser
            };

            await _questApprovalService.SubmitProofAsync(questMemory.ForQuest, proof);
            TempData["QuestUserSuccess"] = "Proof submitted successfully.";
        }
        catch (Exception exception)
        {
            TempData["QuestUserError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { eventId, filter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubmission(int eventId, int questId, string filter)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound();
        }

        try
        {
            var currentUser = await _userService.GetCurrentUser();
            var questMemory = await FindQuestMemoryAsync(currentEvent, currentUser, questId);
            if (questMemory == null || questMemory.Proof == null)
            {
                TempData["QuestUserError"] = "Submission could not be found.";
                return RedirectToAction(nameof(Index), new { eventId, filter });
            }

            await _questApprovalService.DeleteSubmissionAsync(questMemory, currentUser);
            TempData["QuestUserSuccess"] = "Submission deleted successfully.";
        }
        catch (Exception exception)
        {
            TempData["QuestUserError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { eventId, filter });
    }

    private async Task<QuestUserViewModel> BuildViewModelAsync(Event currentEvent, string filter)
    {
        var currentUser = await _userService.GetCurrentUser();
        var isAttending = await _userService.IsAttending(currentEvent);
        var questResults = await new QuestUserCore(_questApprovalService).GetQuestsAsync(currentEvent, currentUser);

        var approvedQuestIds = questResults
            .Where(questMemory => questMemory.ProofStatus == QuestMemoryStatus.Approved)
            .Select(questMemory => questMemory.ForQuest.Id)
            .ToHashSet();

        var allQuests = questResults
            .Select(questMemory =>
            {
                var prerequisite = questMemory.ForQuest.PrerequisiteQuest;
                var isLocked = prerequisite != null && !approvedQuestIds.Contains(prerequisite.Id);
                return new QuestItemViewModel(questMemory, isLocked, isAttending);
            })
            .ToList();

        return new QuestUserViewModel
        {
            EventId = currentEvent.EventId,
            EventName = currentEvent.Name,
            Filter = filter,
            Quests = ApplyFilter(allQuests, filter),
            StatusText = $"{questResults.Count} quest(s) loaded."
        };
    }

    private async Task<QuestMemory?> FindQuestMemoryAsync(Event currentEvent, User currentUser, int questId)
    {
        var questMemories = await _questApprovalService.GetQuestsWithStatus(currentEvent, currentUser);
        return questMemories.FirstOrDefault(questMemory => questMemory.ForQuest.Id == questId);
    }

    private static List<QuestItemViewModel> ApplyFilter(List<QuestItemViewModel> quests, string filter)
    {
        return filter switch
        {
            QuestUserFilters.Submitted => quests
                .Where(quest => quest.Status == QuestMemoryStatus.Submitted)
                .ToList(),
            QuestUserFilters.Completed => quests
                .Where(quest => quest.Status == QuestMemoryStatus.Approved)
                .ToList(),
            QuestUserFilters.Incomplete => quests
                .Where(quest => quest.Status == QuestMemoryStatus.Incomplete)
                .ToList(),
            _ => quests
        };
    }
}
