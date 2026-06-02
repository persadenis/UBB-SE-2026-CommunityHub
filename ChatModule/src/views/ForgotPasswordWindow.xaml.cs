using ChatAndEvents.Data.ChatData.services;
using ChatModule.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

#pragma warning disable SA1300 // Element should begin with upper-case letter
namespace ChatModule.src.views
#pragma warning restore SA1300 // Element should begin with upper-case letter
{
    public sealed partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordViewModel ViewModel { get; }

        public ForgotPasswordWindow(ForgotPasswordViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        public ForgotPasswordWindow(IAuthenticationService authenticationService)
            : this(new ForgotPasswordViewModel(authenticationService))
        {
        }

        public ForgotPasswordWindow(AuthenticationService authenticationService)
            : this(new ForgotPasswordViewModel(authenticationService))
        {
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.NewPassword = NewPasswordBox.Password;
        }
    }
}
