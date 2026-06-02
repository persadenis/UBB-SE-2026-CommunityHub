using ChatAndEvents.Data.ChatData.serviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class ReadReceiptController : ControllerBase
{
    private readonly IReadReceiptService _readReceiptService;

    public ReadReceiptController(IReadReceiptService readReceiptService)
    {
        _readReceiptService = readReceiptService;
    }

    [HttpPost("{conversationId}/mark")]
    public async Task<IActionResult> MarkAsRead(
        Guid conversationId,
        [FromQuery] Guid userId,
        [FromQuery] Guid messageId)
    {
        await _readReceiptService.MarkAsReadAsync(
            conversationId,
            userId,
            messageId);

        return NoContent();
    }

    [HttpPost("{conversationId}/mark-latest")]
    public async Task<IActionResult> MarkLatestAsRead(
        Guid conversationId,
        [FromQuery] Guid userId)
    {
        await _readReceiptService.MarkLatestAsReadAsync(conversationId, userId);
        return NoContent();
    }

    [HttpGet("{conversationId}/{messageId}")]
    public async Task<IActionResult> GetReadReceipts(
        Guid conversationId,
        Guid messageId)
    {
        var receipts = await _readReceiptService
            .GetReadReceiptsAsync(conversationId, messageId);

        return Ok(receipts);
    }

    [HttpGet("{conversationId}/{messageId}/count")]
    public async Task<IActionResult> GetReadByCount(
        Guid conversationId,
        Guid messageId)
    {
        var count = await _readReceiptService
            .GetReadByCountAsync(conversationId, messageId);

        return Ok(count);
    }

    [HttpGet("{conversationId}/{messageId}/others-count")]
    public async Task<IActionResult> GetReadByOthersCount(
        Guid conversationId,
        Guid messageId,
        [FromQuery] Guid currentUserId)
    {
        var count = await _readReceiptService
            .GetReadByOthersCountAsync(conversationId, messageId, currentUserId);

        return Ok(count);
    }

    [HttpGet("{conversationId}/last-read")]
    public async Task<IActionResult> GetLastReadMessage(
        Guid conversationId,
        [FromQuery] Guid userId)
    {
        var messageId = await _readReceiptService
            .GetLastReadMessageAsync(conversationId, userId);

        return Ok(messageId);
    }

    [HttpGet("{conversationId}/participants")]
    public async Task<IActionResult> GetParticipants(Guid conversationId)
    {
        var participants = await _readReceiptService
            .GetParticipantsAsync(conversationId);

        return Ok(participants);
    }

    [HttpGet("{conversationId}/timestamp")]
    public async Task<IActionResult> GetLastReadTimestamp(
        Guid conversationId,
        [FromQuery] Guid userId)
    {
        var timestamp = await _readReceiptService
            .GetLastReadTimestampAsync(conversationId, userId);

        return Ok(timestamp);
    }

    [HttpGet("{conversationId}/{messageId}/readers")]
    public async Task<IActionResult> GetReaderUsernames(
        Guid conversationId,
        Guid messageId,
        [FromQuery] Guid? excludeUserId)
    {
        var readers = await _readReceiptService
            .GetReaderUsernamesAsync(conversationId, messageId, excludeUserId);

        return Ok(readers);
    }
}