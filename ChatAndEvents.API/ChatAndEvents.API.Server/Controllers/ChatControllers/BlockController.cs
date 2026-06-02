using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class BlockController : ControllerBase
{
    private readonly IBlockService _blockService;

    public BlockController(IBlockService blockService)
    {
        _blockService = blockService;
    }

    [HttpPost]
    public async Task<IActionResult> BlockUser(
        [FromQuery] Guid blockerUserId,
        [FromQuery] Guid targetUserId)
    {
        await _blockService.BlockUserAsync(blockerUserId, targetUserId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> UnblockUser(
        [FromQuery] Guid blockerUserId,
        [FromQuery] Guid targetUserId)
    {
        await _blockService.UnblockUserAsync(blockerUserId, targetUserId);
        return NoContent();
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetBlockedUsers(Guid userId)
    {
        var users = await _blockService.GetBlockedUsersAsync(userId);
        return Ok(users);
    }

    [HttpGet("check")]
    public async Task<IActionResult> IsBlocked(
        [FromQuery] Guid blockerId,
        [FromQuery] Guid targetId)
    {
        var result = await _blockService.IsBlockedAsync(blockerId, targetId);
        return Ok(result);
    }
}