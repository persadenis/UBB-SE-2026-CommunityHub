using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.eventStatisticsServices;
using Events_GSS.Services;
using Events_GSS.ViewModels;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Events_GSS.Views;

public sealed partial class EventStatisticsPage : Page
{
    public EventStatisticsViewModel ViewModel { get; private set; } = null!;

    private INavigationService? _nav;
    private bool _usesInjectedViewModel;

    public EventStatisticsPage()
    {
        InitializeComponent();
    }

    public EventStatisticsPage(EventStatisticsViewModel viewModel)
    {
        ViewModel = viewModel;
        _usesInjectedViewModel = true;
        InitializeComponent();
        Loaded += async (_, _) => await InitializeViewModelAsync();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _nav = App.Services.GetRequiredService<INavigationService>();

        if (e.Parameter is not Event ev) return;

        EventNameText.Text = ev.Name;

        var statsService = App.Services.GetRequiredService<IEventStatisticsService>();
        ViewModel = new EventStatisticsViewModel(statsService, ev);

        await InitializeViewModelAsync();
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        if (_usesInjectedViewModel)
        {
            ViewModel.RequestBack();
            return;
        }

        _nav?.GoBack();
    }

    private async Task InitializeViewModelAsync()
    {
        if (ViewModel == null)
        {
            return;
        }

        EventNameText.Text = ViewModel.EventName;
        Bindings.Update();

        await ViewModel.InitializeAsync();

        EngagementRateText.Text = $"{ViewModel.ParticipantOverview.EngagementRate}%";
        ApprovedRateText.Text = $"{ViewModel.EngagementBreakdown.ApprovedQuestsRate}%";
        DeniedRateText.Text = $"{ViewModel.EngagementBreakdown.DeniedQuestsRate}%";
        ApprovedCountText.Text = ViewModel.EngagementBreakdown.ApprovedQuests.ToString();
        DeniedCountText.Text = ViewModel.EngagementBreakdown.DeniedQuests.ToString();
        Bindings.Update();
    }
}
