using ChatAndEvents.Data.EventsData.Services.announcementServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetAnnouncements(int eventId, [FromQuery] Guid userId)
    {
        var announcements = await _announcementService.GetAnnouncementsAsync(eventId, userId);
        return Ok(announcements);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnnouncement([FromQuery] string message, [FromQuery] int eventId, [FromQuery] Guid userId)
    {
        await _announcementService.CreateAnnouncementAsync(message, eventId, userId);
        return NoContent();
    }

    [HttpPut("{announcementId}")]
    public async Task<IActionResult> UpdateAnnouncement(int announcementId, [FromQuery] string newMessage, [FromQuery] Guid userId, [FromQuery] int eventId)
    {
        await _announcementService.UpdateAnnouncementAsync(announcementId, newMessage, userId, eventId);
        return NoContent();
    }

    [HttpDelete("{announcementId}")]
    public async Task<IActionResult> DeleteAnnouncement(int announcementId, [FromQuery] Guid userId, [FromQuery] int eventId)
    {
        await _announcementService.DeleteAnnouncementAsync(announcementId, userId, eventId);
        return NoContent();
    }

    [HttpPost("{announcementId}/pin")]
    public async Task<IActionResult> PinAnnouncement(int announcementId, [FromQuery] int eventId, [FromQuery] Guid userId)
    {
        await _announcementService.PinAnnouncementAsync(announcementId, eventId, userId);
        return NoContent();
    }

    [HttpPost("{announcementId}/read")]
    public async Task<IActionResult> MarkAsRead(int announcementId, [FromQuery] Guid userId)
    {
        var result = await _announcementService.MarkAsReadIfNeededAsync(announcementId, userId, false);
        return Ok(result);
    }

    [HttpGet("{announcementId}/receipts")]
    public async Task<IActionResult> GetReadReceipts(int announcementId, [FromQuery] int eventId, [FromQuery] Guid userId)
    {
        var (readers, total) = await _announcementService.GetReadReceiptsAsync(announcementId, eventId, userId);
        return Ok(new { readers, total });
    }

    [HttpPut("{announcementId}/react")]
    public async Task<IActionResult> AddOrUpdateReaction(int announcementId, [FromQuery] Guid userId, [FromQuery] string emoji)
    {
        await _announcementService.AddOrUpdateReactAsync(announcementId, userId, emoji);
        return NoContent();
    }

    [HttpDelete("{announcementId}/react")]
    public async Task<IActionResult> RemoveReaction(int announcementId, [FromQuery] Guid userId)
    {
        await _announcementService.RemoveReactionAsync(announcementId, userId);
        return NoContent();
    }

    [HttpPost("{announcementId}/react")]
    public async Task<IActionResult> ToggleReaction(int announcementId, [FromQuery] Guid userId, [FromQuery] string emoji)
    {
        await _announcementService.ToggleReactionAsync(announcementId, userId, emoji);
        return NoContent();
    }

    [HttpGet("unread/{userId}")]
    public async Task<IActionResult> GetUnreadCounts(Guid userId)
    {
        var counts = await _announcementService.GetUnreadCountsForUserAsync(userId);
        return Ok(counts);
    }

    [HttpGet("{eventId}/participants")]
    public async Task<IActionResult> GetAllParticipants(int eventId)
    {
        var participants = await _announcementService.GetAllParticipantsAsync(eventId);
        return Ok(participants);
    }

    [HttpGet("{announcementId}/nonreaders")]
    public async Task<IActionResult> GetNonReaders(int announcementId, [FromQuery] int eventId)
    {
        var nonReaders = await _announcementService.GetNonReadersAsync(announcementId, eventId);
        return Ok(nonReaders);
    }
}
