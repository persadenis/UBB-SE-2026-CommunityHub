using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.announcementServices;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Data.EventsData.Services.discussionService;
using ChatAndEvents.Data.EventsData.Services.Interfaces;
using ChatAndEvents.Data.EventsData.Services.userServices;
using Events_GSS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Events_GSS.Views;

public sealed partial class EventDetailPage : Page
{
    // The Wrapper ViewModel we created
    public EventDetailViewModel ViewModel { get; }

    private Event currentEvent;
    private IAttendedEventService? attendedEventService;
    private bool isEnrolled;

    public EventDetailPage(EventDetailViewModel viewModel)
    {
        this.InitializeComponent();

        this.ViewModel = viewModel;
        this.currentEvent = viewModel.SelectedEvent;

        // Bypass OnNavigatedTo and load the tabs immediately
        this.Loaded += async (_, _) => await SetupPageData();
    }

    private async Task SetupPageData()
    {
        // 1. Header Info
        this.EventNameText.Text = currentEvent.Name;
        this.EventInfoText.Text = $"{currentEvent.StartDateTime:MMM dd, yyyy HH:mm} • {currentEvent.Location}";

        // 2. User Authentication & Roles
        var userService = App.Services.GetRequiredService<IUserService>();
        var currentUser = await userService.GetCurrentUser();
        Guid userId = currentUser.UserId;
        bool isAdmin = currentEvent.Admin?.UserId == userId;

        if (isAdmin)
        {
            this.StatisticsButton.Visibility = Visibility.Visible;
            this.JoinLeaveButton.Visibility = Visibility.Collapsed;
        }

        // 3. Announcements Tab
        var announcementService = App.Services.GetRequiredService<IAnnouncementService>();
        var announcementViewModel = new AnnouncementViewModel(currentEvent, announcementService, userId, isAdmin);
        this.AnnouncementTab.ViewModel = announcementViewModel;
        _ = announcementViewModel.InitializeAsync();

        // 4. Discussions Tab
        var discussionService = App.Services.GetRequiredService<IDiscussionService>();
        var discussionViewModel = new DiscussionViewModel(currentEvent, discussionService, userId, isAdmin);
        this.DiscussionTab.ViewModel = discussionViewModel;
        _ = discussionViewModel.InitializeAsync();

        // 5. Quests Tab [FIXED]
        var questAdminVm = new QuestApprovalViewModel(new QuestAdminViewModel(currentEvent));
        var questUserVm = new QuestUserViewModel(currentEvent);

        this.QuestAdminTab.ViewModel = questAdminVm;
        this.QuestUserTab.ViewModel = questUserVm;

        if (isAdmin)
        {
            // Targeting the Container Border, not just the child control!
            this.QuestAdminTabContainer.Visibility = Visibility.Visible;
            this.QuestUserTab.Visibility = Visibility.Collapsed;
        }
        else
        {
            this.QuestAdminTabContainer.Visibility = Visibility.Collapsed;
            this.QuestUserTab.Visibility = Visibility.Visible;
        }

        // 6. Memories Tab
        var memoryService = App.Services.GetRequiredService<IMemoryService>();
        var memoryViewModel = new MemoryViewModel(memoryService);
        this.MemoryTab.ViewModel = memoryViewModel;
        _ = memoryViewModel.InitializeAsync(currentEvent, currentUser);

        // 7. Enrollment Status
        this.attendedEventService = App.Services.GetRequiredService<IAttendedEventService>();

        // Use the ! to guarantee to the compiler that currentEvent is not null
        await this.LoadEnrollmentStatusAsync(currentEvent!, userId);
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        // Tell the MainViewModel to swap the page back to the list
        ViewModel.RequestBack();
    }

    private void OnStatisticsClicked(object sender, RoutedEventArgs e)
    {
        ViewModel.RequestStatistics();
    }

    // --- THEIR ORIGINAL UI LOGIC REMAINS INTACT BELOW ---

    private async Task LoadEnrollmentStatusAsync(Event ev, Guid userId)
    {
        var attendedEvent = await this.attendedEventService!.GetAsync(ev.EventId, userId);
        this.isEnrolled = attendedEvent != null;
        this.JoinLeaveButton.Content = this.isEnrolled ? "Leave Event" : "Join Event";
    }

    private async void OnJoinLeaveClicked(object sender, RoutedEventArgs e)
    {
        if (this.currentEvent == null) return;

        var userService = App.Services.GetRequiredService<IUserService>();
        var userId = (await userService.GetCurrentUser()).UserId;

        try
        {
            this.JoinLeaveButton.IsEnabled = false;

            if (this.isEnrolled)
            {
                await this.attendedEventService!.LeaveEventAsync(this.currentEvent.EventId, userId);
                this.isEnrolled = false;
                this.JoinLeaveButton.Content = "Join Event";
            }
            else
            {
                await this.attendedEventService!.AttendEventAsync(this.currentEvent.EventId, userId);
                this.isEnrolled = true;
                this.JoinLeaveButton.Content = "Leave Event";
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Join/Leave failed: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"Inner: {exception.InnerException?.Message}");
            System.Diagnostics.Debug.WriteLine($"Full: {exception}");
        }
        finally
        {
            this.JoinLeaveButton.IsEnabled = true;
        }
    }
}
