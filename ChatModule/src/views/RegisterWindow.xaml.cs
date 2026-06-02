using System;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.ViewModels;
using Microsoft.UI.Xaml;

#pragma warning disable SA1300 // Element should begin with upper-case letter
namespace ChatModule.src.views
#pragma warning restore SA1300 // Element should begin with upper-case letter
{
    public sealed partial class RegisterWindow : Window
    {
        public RegisterViewModel ViewModel { get; }

        public DateTimeOffset BirthdayDate
        {
            get
            {
                if (ViewModel.Birthday.HasValue)
                {
                    return new DateTimeOffset(ViewModel.Birthday.Value);
                }

                return DateTimeOffset.Now;
            }
            set
            {
                ViewModel.Birthday = value.DateTime;
            }
        }

        public RegisterWindow(RegisterViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        public RegisterWindow(IAuthenticationService authenticationService)
            : this(new RegisterViewModel(authenticationService))
        {
        }

        public bool IsNotLoading(bool isLoading) => !isLoading;

        public Visibility ErrorMessageVisibility(string? errorMessage)
            => string.IsNullOrWhiteSpace(errorMessage) ? Visibility.Collapsed : Visibility.Visible;

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.Password = PasswordBox.Password;
        }
    }
}
