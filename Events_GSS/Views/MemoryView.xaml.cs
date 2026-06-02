// <copyright file="MemoryView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Events_GSS.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Events_GSS.ViewModels;

    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;
    using ChatAndEvents.Data.EventsData.Models;
    using ChatAndEvents.Data.EventsData.ViewModels;
    using Windows.Storage;
    using Windows.Storage.Pickers;

    /// <summary>
    /// Interaction logic for the Memory View control.
    /// </summary>
    public sealed partial class MemoryView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryView"/> class.
        /// </summary>
        public MemoryView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel associated with this view.
        /// </summary>
        public MemoryViewModel ViewModel { get; set; } = null!;

        /// <summary>
        /// Initializes and loads the view model data asynchronously.
        /// </summary>
        /// <param name="ev">The event to load memories for.</param>
        /// <param name="user">The current user viewing the memories.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadAsync(Event ev, User user)
        {
            await this.ViewModel.InitializeAsync(ev, user);
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton btn || btn.DataContext is not MemoryItemViewModel item)
            {
                return;
            }

            await this.ViewModel.ToggleLikeAsync(item);
        }

        private void MyMemoriesToggle_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ShowOnlyMine = this.MyMemoriesToggle.IsChecked == true;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.DataContext is not MemoryItemViewModel item)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Delete Memory",
                Content = "Are you sure you want to delete this memory?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await this.ViewModel.DeleteMemoryAsync(item);
            }
        }

        private async void AddMemoryButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");

            var hwnd = App.MainWindowHandle;
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var files = await picker.PickMultipleFilesAsync();
            if (files.Count == 0)
            {
                return;
            }

            var localPaths = new List<string>();
            foreach (var file in files)
            {
                localPaths.Add(await CopyMemoryImageAsync(file));
            }

            await this.ViewModel.AddPhotoMemoriesAsync(localPaths);
        }

        private static async Task<string> CopyMemoryImageAsync(StorageFile file)
        {
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            var memoryFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ChatModule",
                "MemoryImages");
            Directory.CreateDirectory(memoryFolder);

            var targetPath = Path.Combine(memoryFolder, $"{Guid.NewGuid():N}{extension}");
            await using var sourceStream = await file.OpenStreamForReadAsync();
            await using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);

            return targetPath;
        }
    }
}
