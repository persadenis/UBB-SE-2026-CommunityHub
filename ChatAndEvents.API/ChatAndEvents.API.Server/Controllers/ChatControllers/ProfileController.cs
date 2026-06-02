using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] Guid viewerUserId,
        [FromQuery] string? searchQuery)
    {
        var users = await _profileService.GetAllUsersAsync(viewerUserId, searchQuery);
        return Ok(users);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        var user = await _profileService.GetProfileAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("mutual")]
    public async Task<IActionResult> GetMutualFriends(
        [FromQuery] Guid userId1,
        [FromQuery] Guid userId2)
    {
        var mutuals = await _profileService.GetMutualFriendsAsync(userId1, userId2);
        return Ok(mutuals);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateProfile(
        Guid userId,
        [FromQuery] string? bio,
        [FromQuery] string? avatarUrl,
        [FromQuery] DateTime? birthday)
    {
        await _profileService.UpdateProfileAsync(userId, bio, avatarUrl, birthday);
        return NoContent();
    }

    [HttpPut("{userId}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid userId,
        [FromQuery] UserStatus status)
    {
        await _profileService.UpdateStatusAsync(userId, status);
        return NoContent();
    }
}