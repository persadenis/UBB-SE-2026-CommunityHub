using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class QuestApprovalViewModel
{
    public QuestAdminViewModel QuestAdmin { get; set; } = new();

    public List<QuestMemory> Submissions { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public Quest? SelectedQuest => QuestAdmin.SelectedQuest;
}
