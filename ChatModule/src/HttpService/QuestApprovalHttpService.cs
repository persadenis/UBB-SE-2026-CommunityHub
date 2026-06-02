using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;

namespace ChatAndEvents.Data.EventsData.Services;

public class QuestApprovalHttpService : IQuestApprovalService
{
    private readonly HttpClient _httpClient;

    public QuestApprovalHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SubmitProofAsync(Quest quest, Memory proof)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/QuestApproval/submit",
            new { Quest = quest, Proof = proof });

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<QuestMemory>> GetQuestsWithStatus(Event currentEvent, User user)
    {
        var statuses = await _httpClient.GetFromJsonAsync<List<QuestMemory>>(
            $"api/QuestApproval/{currentEvent.EventId}/status?userId={user.UserId}");

        return statuses ?? new List<QuestMemory>();
    }

    public async Task<List<QuestMemory>> GetProofsForQuestAsync(Quest quest)
    {
        var proofs = await _httpClient.GetFromJsonAsync<List<QuestMemory>>(
            $"api/QuestApproval/{quest.Id}/proofs");

        return proofs ?? new List<QuestMemory>();
    }

    public async Task ChangeProofStatusAsync(QuestMemory proof)
    {
        var response = await _httpClient.PutAsJsonAsync("api/QuestApproval/proof-status", proof);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSubmissionAsync(QuestMemory proof, User user)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/QuestApproval/submission?userId={user.UserId}")
        {
            Content = JsonContent.Create(proof)
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
