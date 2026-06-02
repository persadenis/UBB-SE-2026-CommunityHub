using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.services;

namespace ChatModule.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authenticationService;

    private string username = string.Empty;
    public string Username
    {
        get => username;
        set => Set(ref username, value);
    }

    private string email = string.Empty;
    public string Email
    {
        get => email;
        set => Set(ref email, value);
    }

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set => Set(ref password, value);
    }

    private string phone = string.Empty;
    public string Phone
    {
        get => phone;
        set => Set(ref phone, value);
    }

    private DateTime? birthday;
    public DateTime? Birthday
    {
        get => birthday;
        set => Set(ref birthday, value);
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

    public RelayCommand RegisterCommand { get; }
    public RelayCommand BackToLoginCommand { get; }

    public event Action? NavigateToLoginRequested;
    public event Func<Guid, string, Task>? RegisterSucceeded;

    public RegisterViewModel(IAuthenticationService authenticationService)
    {
        this._authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        RegisterCommand = new RelayCommand(RegisterAsync, () => !IsLoading);
        BackToLoginCommand = new RelayCommand(OnBackToLogin);
    }

    private async Task RegisterAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var user = await _authenticationService.RegisterAsync(
                Username,
                Email,
                Password,
                Phone,
                Birthday,
                null); // avatarUrl

            if (RegisterSucceeded != null)
            {
                await RegisterSucceeded(user.Id, user.Username);
            }
            else
            {
                NavigateToLoginRequested?.Invoke();
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

    private Task OnBackToLogin()
    {
        NavigateToLoginRequested?.Invoke();
        return Task.CompletedTask;
    }
}
