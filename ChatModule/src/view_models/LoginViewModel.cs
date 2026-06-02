using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.ViewModels;

namespace ChatModule.viewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;

    private string username = string.Empty;
    public string Username
    {
        get => username;
        set => Set(ref username, value);
    }

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set => Set(ref password, value);
    }

    private string? errorMessage;
    public string? ErrorMessage
    {
        get => errorMessage;
        set => Set(ref errorMessage, value);
    }

    private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set => Set(ref isLoading, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand GoToRegisterCommand { get; }
        public RelayCommand ForgotPasswordCommand { get; }

        public event Func<Guid, string, Task>? LoginSucceeded;
        public event Action? RegisterRequested;
        public event Action? ForgotPasswordRequested;

        public LoginViewModel(IAuthenticationService authenticationService)
        {
            this._authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

            LoginCommand = new RelayCommand(LoginAsync, () => !IsLoading);
            GoToRegisterCommand = new RelayCommand(OnGoToRegister);
            ForgotPasswordCommand = new RelayCommand(OnForgotPassword);
        }

        private async Task LoginAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                var user = await _authenticationService.LoginAsync(Username, Password);
                if (user != null)
                {
                    if (LoginSucceeded != null)
                {
                    await LoginSucceeded(user.Id, user.Username);
                }
            }
                else
                {
                    ErrorMessage = "Invalid credentials";
                }
            }
            catch (Exception exception)
        {
                ErrorMessage = exception.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Task OnGoToRegister()
        {
            RegisterRequested?.Invoke();
            return Task.CompletedTask;
        }

        private Task OnForgotPassword()
        {
            ForgotPasswordRequested?.Invoke();
            return Task.CompletedTask;
    }
}