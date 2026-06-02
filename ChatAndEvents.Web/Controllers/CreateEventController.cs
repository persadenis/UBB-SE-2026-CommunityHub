using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.EventsData.ViewModelsCore;
using ChatAndEvents.Web.Extensions;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class CreateEventController : Controller
{
    private const string WizardStateSessionKey = "CreateEventWizardState";

    private readonly IUserService _userService;
    private readonly IEventService _eventService;
    private readonly IQuestService _questService;
    private readonly IAttendedEventService _attendedEventService;

    public CreateEventController(
        IUserService userService,
        IEventService eventService,
        IQuestService questService,
        IAttendedEventService attendedEventService)
    {
        _userService = userService;
        _eventService = eventService;
        _questService = questService;
        _attendedEventService = attendedEventService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await LoadViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GoToStep2(CreateEventViewModel postedModel)
    {
        var model = await LoadViewModelAsync();
        ApplyStep1(postedModel, model);

        var core = BuildCore(model);
        core.GoToStep2();

        model.ErrorMessage = core.ErrorMessage;
        model.CurrentStep = string.IsNullOrWhiteSpace(core.ErrorMessage) ? 2 : 1;

        await SaveViewModelAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GoToStep3(CreateEventViewModel postedModel)
    {
        var model = await LoadViewModelAsync();
        ApplyStep2(postedModel, model);
        model.ErrorMessage = null;
        model.CurrentStep = 3;

        await SaveViewModelAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GoBackToStep1(CreateEventViewModel postedModel)
    {
        var model = await LoadViewModelAsync();
        ApplyStep2(postedModel, model);
        model.ErrorMessage = null;
        model.CurrentStep = 1;

        await SaveViewModelAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GoBackToStep2(CreateEventViewModel postedModel)
    {
        var model = await LoadViewModelAsync();
        model.ErrorMessage = null;
        model.CurrentStep = 2;

        await SaveViewModelAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleQuest(int questId)
    {
        var model = await LoadViewModelAsync();
        model.CurrentStep = 3;
        model.ErrorMessage = null;

        var existingQuest = model.SelectedQuests.FirstOrDefault(quest => quest.PresetQuestId == questId);
        if (existingQuest != null)
        {
            model.SelectedQuests.Remove(existingQuest);
        }
        else
        {
            var presetQuest = model.AvailableQuests.FirstOrDefault(quest => quest.Id == questId);
            if (presetQuest != null)
            {
                model.SelectedQuests.Add(new CreateEventSelectedQuestViewModel
                {
                    PresetQuestId = presetQuest.Id,
                    Name = presetQuest.Name,
                    Description = presetQuest.Description,
                    Difficulty = presetQuest.Difficulty
                });
            }
        }

        await SaveViewModelAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCustomQuest(CreateEventViewModel postedModel)
    {
        var model = await LoadViewModelAsync();
        model.CustomQuestName = postedModel.CustomQuestName;
        model.CustomQuestDescription = postedModel.CustomQuestDescription;
        model.CurrentStep = 3;
        model.ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(model.CustomQuestName) ||
            string.IsNullOrWhiteSpace(model.CustomQuestDescription))
        {
            model.ErrorMessage = "Both custom quest name and description are required.";
            return View("Index", model);
        }

        model.SelectedQuests.Add(new CreateEventSelectedQuestViewModel
        {
            Name = model.CustomQuestName.Trim(),
            Description = model.CustomQuestDescription.Trim(),
            Difficulty = 3
        });

        model.CustomQuestName = string.Empty;
        model.CustomQuestDescription = string.Empty;

        await SaveViewModelAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveQuest(string key)
    {
        var model = await LoadViewModelAsync();
        model.CurrentStep = 3;
        model.ErrorMessage = null;

        var quest = model.SelectedQuests.FirstOrDefault(item => item.Key == key);
        if (quest != null)
        {
            model.SelectedQuests.Remove(quest);
        }

        await SaveViewModelAsync(model);
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel()
    {
        HttpContext.Session.Remove(WizardStateSessionKey);
        return RedirectToAction("Index", "EventListing");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventViewModel postedModel)
    {
        var model = await LoadViewModelAsync();
        model.CurrentStep = 3;
        model.ErrorMessage = null;

        try
        {
            var dto = await BuildCore(model).BuildDto();

            var eventEntity = new Event
            {
                Name = dto.Name,
                Location = dto.Location,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                IsPublic = dto.IsPublic,
                Description = dto.Description,
                MaximumPeople = dto.MaximumPeople,
                EventBannerPath = dto.EventBannerPath,
                Category = dto.Category,
                Admin = dto.Admin
            };

            var newEventId = await _eventService.CreateEventAsync(eventEntity);
            eventEntity.EventId = newEventId;

            var currentUser = await _userService.GetCurrentUser();
            await _attendedEventService.AttendEventAsync(newEventId, currentUser.UserId);

            foreach (var quest in dto.SelectedQuests)
            {
                await _questService.AddQuestAsync(eventEntity, quest);
            }

            HttpContext.Session.Remove(WizardStateSessionKey);
            TempData["EventDetailSuccess"] = "Event created successfully.";
            return RedirectToAction("Index", "EventDetail", new { eventId = newEventId });
        }
        catch (Exception exception)
        {
            model.ErrorMessage = exception.Message;
            await SaveViewModelAsync(model);
            return View("Index", model);
        }
    }

    private async Task<CreateEventViewModel> LoadViewModelAsync()
    {
        var availableQuests = await _questService.GetPresetQuestsAsync();
        var state = HttpContext.Session.GetObject<CreateEventWizardState>(WizardStateSessionKey);

        return state == null
            ? new CreateEventViewModel { AvailableQuests = availableQuests }
            : CreateEventViewModel.FromState(state, availableQuests);
    }

    private Task SaveViewModelAsync(CreateEventViewModel model)
    {
        HttpContext.Session.SetObject(WizardStateSessionKey, model.ToState());
        return Task.CompletedTask;
    }

    private CreateEventViewModelCore BuildCore(CreateEventViewModel model)
    {
        var core = new CreateEventViewModelCore(
            _userService,
            _eventService,
            _questService,
            _attendedEventService);

        core.SetEventName(model.EventName);
        core.SetLocation(model.Location);
        core.SetStartDate(model.StartDate);
        core.SetStartTime(model.StartTime);
        core.SetEndDate(model.EndDate);
        core.SetEndTime(model.EndTime);
        core.SetIsPublic(model.IsPublic);
        core.SetDescription(model.Description);
        core.SetMaximumPeopleText(model.MaximumPeopleText);
        core.SetEventBannerPath(model.EventBannerPath);
        core.SetSelectedCategory(model.SelectedCategory);

        foreach (var quest in BuildSelectedQuests(model))
        {
            core.ToggleQuestSelection(quest);
        }

        return core;
    }

    private static void ApplyStep1(CreateEventViewModel source, CreateEventViewModel target)
    {
        target.EventName = source.EventName;
        target.Location = source.Location;
        target.StartDate = source.StartDate;
        target.StartTime = source.StartTime;
        target.EndDate = source.EndDate;
        target.EndTime = source.EndTime;
        target.IsPublic = source.IsPublic;
    }

    private static void ApplyStep2(CreateEventViewModel source, CreateEventViewModel target)
    {
        target.Description = source.Description;
        target.MaximumPeopleText = source.MaximumPeopleText;
        target.EventBannerPath = source.EventBannerPath;
        target.SelectedCategoryId = source.SelectedCategoryId;
    }

    private static List<Quest> BuildSelectedQuests(CreateEventViewModel model)
    {
        var presetQuestsById = model.AvailableQuests.ToDictionary(quest => quest.Id);
        var quests = new List<Quest>();

        foreach (var selectedQuest in model.SelectedQuests)
        {
            if (selectedQuest.PresetQuestId.HasValue &&
                presetQuestsById.TryGetValue(selectedQuest.PresetQuestId.Value, out var presetQuest))
            {
                quests.Add(new Quest
                {
                    Id = presetQuest.Id,
                    Name = presetQuest.Name,
                    Description = presetQuest.Description,
                    Difficulty = presetQuest.Difficulty,
                    PrerequisiteQuest = presetQuest.PrerequisiteQuest
                });
                continue;
            }

            quests.Add(new Quest
            {
                Name = selectedQuest.Name,
                Description = selectedQuest.Description,
                Difficulty = selectedQuest.Difficulty
            });
        }

        return quests;
    }
}
