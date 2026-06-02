using ChatAndEvents.Data.EventsData.Services.notificationServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> Notify(
        [FromQuery] Guid userId,
        [FromQuery] string title,
        [FromQuery] string description,
        [FromQuery] string type = "General",
        [FromQuery] string sourceFeature = "System",
        [FromQuery] string? sourceEntityId = null)
    {
        await _notificationService.NotifyAsync(userId, title, description, type, sourceFeature, sourceEntityId);
        return NoContent();
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetNotifications(Guid userId)
    {
        var notifications = await _notificationService.GetNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("{userId}/unread-count")]
    public async Task<IActionResult> CountUnread(Guid userId)
    {
        var count = await _notificationService.CountUnreadAsync(userId);
        return Ok(count);
    }

    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId, [FromQuery] Guid userId)
    {
        await _notificationService.MarkAsReadAsync(notificationId, userId);
        return NoContent();
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(int notificationId)
    {
        await _notificationService.DeleteAsync(notificationId);
        return NoContent();
    }
}
