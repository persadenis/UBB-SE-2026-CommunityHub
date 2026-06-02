using Events_GSS.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.UI.Xaml.Navigation;

namespace Events_GSS.Views
{
    public sealed partial class MyEventsPage : Page
    {
        public MyEventsViewModel ViewModel { get; }

        public MyEventsPage(MyEventsViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            this.MyEventsListView.ItemsSource = ViewModel.Events;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadMyEventsAsync();
        }

        private void OnEventTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is Event selectedEvent)
            {
                ViewModel.RequestEventDetails(selectedEvent);
            }
        }
    }
}
