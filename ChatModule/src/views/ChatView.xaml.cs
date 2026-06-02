using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChatModule.src.view_models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Storage.Pickers;
using ChatModule.src.view_models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatModule.src.views
{
    public sealed partial class ChatView : UserControl
    {
        private const ulong MaxImageSizeBytes = 6UL * 1024UL * 1024UL; // 6 MB
        private const int DaysInOneWeek = 7;
        private const int DaysInTwoWeeks = 14;
        private const int DaysInOneMonth = 30;

        private static readonly string[] ReactionEmojis = new[] { "👍", "❤️", "😂", "🔥", "👏", "😮", "😢", "🙏", "🎉", "👀" };

        public ChatView(ChatViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.InitializeComponent();

            this.ViewModel.RequestEmojiAsync = this.RequestEmojiAsync;
            this.ViewModel.RequestPinExpiryAsync = this.RequestPinExpiryAsync;

            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
            this.ViewModel.ScrollToMessageRequested += this.OnScrollToMessageRequested;
            this.ViewModel.ReadReceiptDetailsRequested += this.OnReadReceiptDetailsRequested;
            this.ViewModel.ReplyPreviewTapped += this.OnReplyPreviewTappedByViewModel;
            this.ViewModel.Messages.CollectionChanged += this.OnMessagesCollectionChanged;
        }

        public ChatViewModel ViewModel { get; }

        public void SetSidePanel(UserControl panel)
        {
            this.SidePanelHost.Content = panel;
        }

        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            this.AttachSearchPanel();
            _ = this.ViewModel.MarkConversationAsReadAsync();
        }

        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            this.ViewModel.ScrollToMessageRequested -= this.OnScrollToMessageRequested;
            this.ViewModel.ReadReceiptDetailsRequested -= this.OnReadReceiptDetailsRequested;
            this.ViewModel.ReplyPreviewTapped -= this.OnReplyPreviewTappedByViewModel;
            this.ViewModel.Messages.CollectionChanged -= this.OnMessagesCollectionChanged;
            this.Loaded -= this.OnLoaded;
            this.Unloaded -= this.OnUnloaded;
        }

        private void OnSearchPanelLoaded(object sender, RoutedEventArgs eventArgs)
        {
            this.AttachSearchPanel();
        }

        private void AttachSearchPanel()
        {
            if (this.SearchPanelHost.Content != null)
            {
                return;
            }

            this.SearchPanelHost.Content = new MessageSearchPanel(this.ViewModel.MessageSearch);
        }

        private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (this.ViewModel.Messages.Count > 0 && eventArgs?.Action == NotifyCollectionChangedAction.Add && eventArgs.NewItems != null)
            {
                foreach (var item in eventArgs.NewItems)
                {
                    if (item is Message newMessage && !newMessage.IsMine)
                    {
                        _ = this.ViewModel.MarkVisibleMessagesAsReadAsync(newMessage.Id);
                    }
                }

                this.ScrollToBottom();
            }
        }

        private void OnScrollToMessageRequested(Guid messageId)
        {
            var target = this.ViewModel.Messages.FirstOrDefault(message => message.Id == messageId);
            if (target != null)
            {
                this.MessagesList.ScrollIntoView(target);
            }
        }

        private void ScrollToBottom()
        {
            if (this.ViewModel.Messages.Count == 0)
            {
                return;
            }

            var lastMessage = this.ViewModel.Messages[^1];
            this.MessagesList.ScrollIntoView(lastMessage);
        }

        private async void OnLeaveGroupClicked(object sender, RoutedEventArgs eventArgs)
        {
            await this.ViewModel.LeaveGroupAsync();
        }

        private async void OnAttachClicked(object sender, RoutedEventArgs eventArgs)
        {
            if (App.MainAppWindow == null)
            {
                return;
            }

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");

            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, windowHandle);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var fileExtension = Path.GetExtension(file.Name)?.ToLowerInvariant();
                if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg")
                {
                    await this.ShowInfoDialogAsync("Attachment", "Only PNG and JPEG images are supported.");
                    return;
                }

                var fileProperties = await file.GetBasicPropertiesAsync();
                if (fileProperties.Size > MaxImageSizeBytes)
                {
                    await this.ShowInfoDialogAsync("Attachment", "Image size must be 6MB or less.");
                    return;
                }

                await this.ViewModel.SetAttachmentAsync(file.Path);
            }
        }

        private async void OnClearAttachmentClicked(object sender, RoutedEventArgs eventArgs)
        {
            await this.ViewModel.ClearAttachmentAsync();
        }

        private async void OnSetNicknameClicked(object sender, RoutedEventArgs eventArgs)
        {
            await this.ViewModel.SetNicknameAsync();
        }

        private async void OnClearNicknameClicked(object sender, RoutedEventArgs eventArgs)
        {
            await this.ViewModel.ClearNicknameAsync();
        }

        private async void OnReadReceiptTapped(object sender, TappedRoutedEventArgs eventArgs)
        {
            if (sender is TextBlock { Tag: Guid messageId })
            {
                await this.ViewModel.ShowReadReceiptDetailsAsync(messageId);
            }
        }

        private async void OnReplyPreviewTapped(object sender, TappedRoutedEventArgs eventArgs)
        {
            if (sender is Button button && button.Tag is Guid replyToId && replyToId != Guid.Empty)
            {
                await this.ViewModel.OpenReplyTargetAsync(replyToId);
            }
        }

        private void OnReplyPreviewTappedByViewModel(Guid replyToId)
        {
            this.OnScrollToMessageRequested(replyToId);
        }

        private async void OnReadReceiptDetailsRequested(string body)
        {
            if (this.XamlRoot == null)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Seen By",
                Content = body,
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            _ = await dialog.ShowAsync();
        }

        private async Task<string?> RequestEmojiAsync()
        {
            if (this.XamlRoot == null)
            {
                return null;
            }

            var list = new ListView
            {
                SelectionMode = ListViewSelectionMode.Single,
                IsItemClickEnabled = true,
                ItemsSource = ReactionEmojis,
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 23, 21, 59)),
                Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 200, 172, 214)),
                Width = 280,
                MaxHeight = 220
            };

            var selectedEmoji = default(string);
            list.ItemClick += (_, eventArgs) =>
            {
                selectedEmoji = eventArgs.ClickedItem as string;
            };

            var dialog = new ContentDialog
            {
                Title = "Pick a reaction",
                Content = list,
                PrimaryButtonText = "Use",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(selectedEmoji) ? list.SelectedItem as string : selectedEmoji;
        }

        private async Task<DateTime?> RequestPinExpiryAsync()
        {
            if (this.XamlRoot == null)
            {
                return null;
            }

            var radioButtonOneWeek = new RadioButton
            {
                Content = "1 Week",
                IsChecked = true,
                Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 200, 172, 214))
            };
            var radioButtonTwoWeeks = new RadioButton
            {
                Content = "2 Weeks",
                Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 200, 172, 214))
            };
            var radioButtonOneMonth = new RadioButton
            {
                Content = "1 Month",
                Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 200, 172, 214))
            };
            var radioButtonCustom = new RadioButton
            {
                Content = "Custom",
                Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 200, 172, 214))
            };

            var datePicker = new CalendarDatePicker
            {
                PlaceholderText = "Select date",
                MinDate = DateTimeOffset.UtcNow.AddDays(1),
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var timePicker = new TimePicker
            {
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 4, 0, 0)
            };

            radioButtonCustom.Checked += (_, _) =>
            {
                datePicker.Visibility = Visibility.Visible;
                timePicker.Visibility = Visibility.Visible;
            };
            radioButtonOneWeek.Checked += (_, _) =>
            {
                datePicker.Visibility = Visibility.Collapsed;
                timePicker.Visibility = Visibility.Collapsed;
            };
            radioButtonTwoWeeks.Checked += (_, _) =>
            {
                datePicker.Visibility = Visibility.Collapsed;
                timePicker.Visibility = Visibility.Collapsed;
            };
            radioButtonOneMonth.Checked += (_, _) =>
            {
                datePicker.Visibility = Visibility.Collapsed;
                timePicker.Visibility = Visibility.Collapsed;
            };

            var panel = new StackPanel { Spacing = 6 };
            panel.Children.Add(radioButtonOneWeek);
            panel.Children.Add(radioButtonTwoWeeks);
            panel.Children.Add(radioButtonOneMonth);
            panel.Children.Add(radioButtonCustom);
            panel.Children.Add(datePicker);
            panel.Children.Add(timePicker);

            var dialog = new ContentDialog
            {
                Title = "Pin duration",
                Content = panel,
                PrimaryButtonText = "Pin",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return null;
            }

            if (radioButtonOneWeek.IsChecked == true)
            {
                return DateTime.UtcNow.AddDays(DaysInOneWeek);
            }

            if (radioButtonTwoWeeks.IsChecked == true)
            {
                return DateTime.UtcNow.AddDays(DaysInTwoWeeks);
            }

            if (radioButtonOneMonth.IsChecked == true)
            {
                return DateTime.UtcNow.AddDays(DaysInOneMonth);
            }

            // Custom selection fallback
            if (datePicker.Date == null)
            {
                return DateTime.UtcNow.AddDays(DaysInOneWeek);
            }

            var chosenDateTime = datePicker.Date.Value.Date + timePicker.Time;
            var chosenUtcDateTime = DateTime.SpecifyKind(chosenDateTime, DateTimeKind.Local).ToUniversalTime();

            if (chosenUtcDateTime <= DateTime.UtcNow)
            {
                chosenUtcDateTime = DateTime.UtcNow.AddDays(DaysInOneWeek);
            }

            return chosenUtcDateTime;
        }

        private async Task ShowInfoDialogAsync(string title, string body)
        {
            if (this.XamlRoot == null)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = body,
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            _ = await dialog.ShowAsync();
        }
    }
}