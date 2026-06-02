using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.EventsData.ViewModelsCore;

namespace Events_GSS.ViewModels
{
    // Test double to avoid loading WinUI/WindowsAppSDK in unit tests.
    public class QuestUserViewModel
    {
        private readonly QuestUserCore core;
        private readonly Event currentEvent;
        private readonly IUserService userService;

        public ObservableCollection<TestQuestItem> Quests { get; } = new ObservableCollection<TestQuestItem>();
        public bool IsLoading { get; private set; }
        public string StatusText { get; private set; } = string.Empty;

        public QuestUserViewModel(Event currentEvent, IQuestApprovalService questApprovalService, IUserService userService)
        {
            this.currentEvent = currentEvent;
            this.core = new QuestUserCore(questApprovalService);
            this.userService = userService;
        }

        public async Task LoadQuestsAsync()
        {
            IsLoading = true;
            Quests.Clear();
            var currentUser = userService.GetCurrentUser();
            var results = await core.GetQuestsAsync(currentEvent, currentUser);
            foreach (var qm in results)
            {
                Quests.Add(new TestQuestItem { Name = qm.ForQuest.Name });
            }
            StatusText = $"{results.Count} quest(s) loaded.";
            IsLoading = false;
        }
    }

    public class TestQuestItem
    {
        public string? Name { get; set; }
    }
}