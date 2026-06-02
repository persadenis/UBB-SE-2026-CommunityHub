using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class QuestAdminController : Controller
{
    private readonly IEventService _eventService;
    private readonly IQuestService _questService;

    public QuestAdminController(IEventService eventService, IQuestService questService)
    {
        _eventService = eventService;
        _questService = questService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCustom(QuestAdminViewModel model)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(model.EventId);
        if (currentEvent == null)
        {
            return NotFound();
        }

        try
        {
            if (string.IsNullOrWhiteSpace(model.NewQuestName) ||
                string.IsNullOrWhiteSpace(model.NewQuestDescription))
            {
                TempData["QuestApprovalError"] = "Quest name and description are required.";
                return RedirectToAction("Index", "QuestApproval", new { eventId = model.EventId, selectedQuestId = model.SelectedQuestId });
            }

            if (model.NewQuestDifficulty is < 1 or > 5)
            {
                TempData["QuestApprovalError"] = "Difficulty must be between 1 and 5.";
                return RedirectToAction("Index", "QuestApproval", new { eventId = model.EventId, selectedQuestId = model.SelectedQuestId });
            }

            var existingQuests = await _questService.GetQuestsAsync(currentEvent);
            var prerequisiteQuest = model.SelectedPrerequisiteQuestId.HasValue
                ? existingQuests.FirstOrDefault(quest => quest.Id == model.SelectedPrerequisiteQuestId.Value)
                : null;

            var quest = new Quest
            {
                Name = model.NewQuestName.Trim(),
                Description = model.NewQuestDescription.Trim(),
                Difficulty = model.NewQuestDifficulty,
                PrerequisiteQuest = prerequisiteQuest
            };

            var newQuestId = await _questService.AddQuestAsync(currentEvent, quest);
            TempData["QuestApprovalSuccess"] = "Custom quest added.";
            return RedirectToAction("Index", "QuestApproval", new { eventId = model.EventId, selectedQuestId = newQuestId });
        }
        catch (Exception exception)
        {
            TempData["QuestApprovalError"] = exception.Message;
            return RedirectToAction("Index", "QuestApproval", new { eventId = model.EventId, selectedQuestId = model.SelectedQuestId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPreset(QuestAdminViewModel model)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(model.EventId);
        if (currentEvent == null)
        {
            return NotFound();
        }

        if (!model.SelectedPresetQuestId.HasValue)
        {
            TempData["QuestApprovalError"] = "Select a preset quest first.";
            return RedirectToAction("Index", "QuestApproval", new { eventId = model.EventId, selectedQuestId = model.SelectedQuestId });
        }

        try
        {
            var presetQuest = (await _questService.GetPresetQuestsAsync())
                .FirstOrDefault(quest => quest.Id == model.SelectedPresetQuestId.Value);

            if (presetQuest == null)
            {
                TempData["QuestApprovalError"] = "Preset quest could not be found.";
                return RedirectToAction("Index", "QuestApproval", new { eventId = model.EventId, selectedQuestId = model.SelectedQuestId });
            }

            var newQuestId = await _questService.AddQuestAsync(currentEvent, presetQuest);
            TempData["QuestApprovalSuccess"] = "Preset quest added.";
            return RedirectToAction("Index", "QuestApproval", new { eventId = model.EventId, selectedQuestId = newQuestId });
        }
        catch (Exception exception)
        {
            TempData["QuestApprovalError"] = exception.Message;
            return RedirectToAction("Index", "QuestApproval", new { eventId = model.EventId, selectedQuestId = model.SelectedQuestId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int eventId, int questId, int? selectedQuestId)
    {
        var currentEvent = await _eventService.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound();
        }

        try
        {
            var quest = (await _questService.GetQuestsAsync(currentEvent))
                .FirstOrDefault(item => item.Id == questId);
            if (quest == null)
            {
                TempData["QuestApprovalError"] = "Quest could not be found.";
                return RedirectToAction("Index", "QuestApproval", new { eventId, selectedQuestId });
            }

            await _questService.DeleteQuestAsync(quest);
            TempData["QuestApprovalSuccess"] = "Quest deleted.";
        }
        catch (Exception exception)
        {
            TempData["QuestApprovalError"] = exception.Message;
        }

        var redirectSelectedQuestId = selectedQuestId == questId ? null : selectedQuestId;
        return RedirectToAction("Index", "QuestApproval", new { eventId, selectedQuestId = redirectSelectedQuestId });
    }
}
