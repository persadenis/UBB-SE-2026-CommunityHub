using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Events_GSS.ViewModels;
using ChatAndEvents.Data.EventsData.Models;

namespace Events_GSS.Views;

public sealed partial class EventListingPage : Page
{
    public EventListingViewModel ViewModel { get; }

    public EventListingPage(EventListingViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        this.EventsListView.ItemsSource = ViewModel.Events;
    }

    private void OnEventTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is Event selectedEvent)
        {
            // Tell the ViewModel the user wants to see details
            ViewModel.RequestEventDetails(selectedEvent);
        }
    }

    private void OnCreateEventClicked(object sender, RoutedEventArgs e)
    {
        // Tell the ViewModel the user wants to create an event
        ViewModel.RequestCreateEvent();
    }
}