using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class QuestApprovalController : Controller
{
    private readonly IEventService _eventService;
    private readonly IQuestService _questService;
    private readonly IQuestApprovalService _questApprovalService;

    public QuestApprovalController(
        IEventService eventService,
        IQuestService questService,
        IQuestApprovalService questApprovalService)
    {
        _eventService = eventService;
        _questService = questService;
        _questApprovalService = questApprovalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId, int? selectedQuestId)
    {
        var viewModel = await BuildViewModelAsync(eventId, selectedQuestId);
        if (viewModel == null)
        {
            return NotFound();
        }

        if (TempData["QuestApprovalError"] is string errorMessage)
        {
            viewModel.ErrorMessage = errorMessage;
            viewModel.QuestAdmin.ErrorMessage = errorMessage;
        }

        if (TempData["QuestApprovalSuccess"] is string successMessage)
        {
            viewModel.SuccessMessage = successMessage;
            viewModel.QuestAdmin.SuccessMessage = successMessage;
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SelectQuest(int eventId, int selectedQuestId)
    {
        return RedirectToAction(nameof(Index), new { eventId, selectedQuestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int eventId, int selectedQuestId, int memoryId)
    {
        return await UpdateProofStatusAsync(
            eventId,
            selectedQuestId,
            memoryId,
            QuestMemoryStatus.Approved,
            "Submission approved.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deny(int eventId, int selectedQuestId, int memoryId)
    {
        return await UpdateProofStatusAsync(
            eventId,
            selectedQuestId,
            memoryId,
            QuestMemoryStatus.Rejected,
            "Submission denied.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubmission(int eventId, int selectedQuestId, int memoryId)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound();
        }

        try
        {
            var selectedQuest = (await _questService.GetQuestsAsync(currentEvent))
                .FirstOrDefault(quest => quest.Id == selectedQuestId);
            if (selectedQuest == null)
            {
                TempData["QuestApprovalError"] = "Quest could not be found.";
                return RedirectToAction(nameof(Index), new { eventId, selectedQuestId });
            }

            var proof = (await _questApprovalService.GetProofsForQuestAsync(selectedQuest))
                .FirstOrDefault(submission => submission.MemoryId == memoryId);

            if (proof == null || proof.Proof?.Author == null)
            {
                TempData["QuestApprovalError"] = "Submission could not be found.";
                return RedirectToAction(nameof(Index), new { eventId, selectedQuestId });
            }

            await _questApprovalService.DeleteSubmissionAsync(proof, proof.Proof.Author);
            TempData["QuestApprovalSuccess"] = "Submission deleted.";
        }
        catch (Exception exception)
        {
            TempData["QuestApprovalError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { eventId, selectedQuestId });
    }

    private async Task<IActionResult> UpdateProofStatusAsync(
        int eventId,
        int selectedQuestId,
        int memoryId,
        QuestMemoryStatus status,
        string successMessage)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound();
        }

        try
        {
            var selectedQuest = (await _questService.GetQuestsAsync(currentEvent))
                .FirstOrDefault(quest => quest.Id == selectedQuestId);
            if (selectedQuest == null)
            {
                TempData["QuestApprovalError"] = "Quest could not be found.";
                return RedirectToAction(nameof(Index), new { eventId, selectedQuestId });
            }

            var proof = (await _questApprovalService.GetProofsForQuestAsync(selectedQuest))
                .FirstOrDefault(submission => submission.MemoryId == memoryId);

            if (proof == null)
            {
                TempData["QuestApprovalError"] = "Submission could not be found.";
                return RedirectToAction(nameof(Index), new { eventId, selectedQuestId });
            }

            proof.ProofStatus = status;
            await _questApprovalService.ChangeProofStatusAsync(proof);
            TempData["QuestApprovalSuccess"] = successMessage;
        }
        catch (Exception exception)
        {
            TempData["QuestApprovalError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { eventId, selectedQuestId });
    }

    private async Task<QuestApprovalViewModel?> BuildViewModelAsync(int eventId, int? selectedQuestId)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return null;
        }

        var eventQuests = await _questService.GetQuestsAsync(currentEvent);
        var presetQuests = await _questService.GetPresetQuestsAsync();
        var resolvedSelectedQuestId = selectedQuestId.HasValue && eventQuests.Any(quest => quest.Id == selectedQuestId.Value)
            ? selectedQuestId
            : null;

        var submissions = new List<QuestMemory>();
        if (resolvedSelectedQuestId.HasValue)
        {
            var selectedQuest = eventQuests.First(quest => quest.Id == resolvedSelectedQuestId.Value);
            submissions = await _questApprovalService.GetProofsForQuestAsync(selectedQuest);
        }

        return new QuestApprovalViewModel
        {
            QuestAdmin = new QuestAdminViewModel
            {
                EventId = currentEvent.EventId,
                EventName = currentEvent.Name,
                Quests = eventQuests,
                PresetQuests = presetQuests,
                SelectedQuestId = resolvedSelectedQuestId
            },
            Submissions = submissions
        };
    }
}
