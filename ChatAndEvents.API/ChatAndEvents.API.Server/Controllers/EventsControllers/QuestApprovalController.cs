using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

public class SubmitProofRequest
{
    public Quest Quest { get; set; } = null!;
    public Memory Proof { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class QuestApprovalController : ControllerBase
{
    private readonly IQuestApprovalService _questApprovalService;

    public QuestApprovalController(IQuestApprovalService questApprovalService)
    {
        _questApprovalService = questApprovalService;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitProof([FromBody] SubmitProofRequest request)
    {
        await _questApprovalService.SubmitProofAsync(request.Quest, request.Proof);
        return NoContent();
    }

    [HttpGet("{eventId}/status")]
    public async Task<IActionResult> GetQuestsWithStatus(
        int eventId,
        [FromQuery] Guid userId)
    {
        var ev = new Event { EventId = eventId };
        var user = new User { UserId = userId };

        var result = await _questApprovalService
            .GetQuestsWithStatus(ev, user);

        return Ok(result);
    }

    [HttpGet("{questId}/proofs")]
    public async Task<IActionResult> GetProofsForQuest(int questId)
    {
        var quest = new Quest { Id = questId };

        var proofs = await _questApprovalService
            .GetProofsForQuestAsync(quest);

        return Ok(proofs);
    }

    [HttpPut("proof-status")]
    public async Task<IActionResult> ChangeProofStatus(
        [FromBody] QuestMemory proof)
    {
        await _questApprovalService.ChangeProofStatusAsync(proof);
        return NoContent();
    }

    [HttpDelete("submission")]
    public async Task<IActionResult> DeleteSubmission(
        [FromBody] QuestMemory proof,
        [FromQuery] Guid userId)
    {
        var user = new User { UserId = userId };

        await _questApprovalService.DeleteSubmissionAsync(proof, user);

        return NoContent();
    }
}