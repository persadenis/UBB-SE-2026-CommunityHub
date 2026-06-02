using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Services.achievementServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class ReputationController : Controller
{
    private readonly IUserService _userService;
    private readonly IReputationService _reputationService;
    private readonly IAchievementService _achievementService;

    public ReputationController(
        IUserService userService,
        IReputationService reputationService,
        IAchievementService achievementService)
    {
        _userService = userService;
        _reputationService = reputationService;
        _achievementService = achievementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userService.GetCurrentUser();
        var reputationScore = await _reputationService.GetReputationScoreAsync(currentUser.UserId);
        var achievements = await _achievementService.GetUserAchievementsAsync(currentUser.UserId);

        var viewModel = new ReputationViewModel
        {
            UserName = currentUser.Name,
            ReputationPoints = reputationScore.ReputationPoints,
            CurrentTier = reputationScore.Tier,
            Achievements = achievements,
        };

        return View(viewModel);
    }
}
