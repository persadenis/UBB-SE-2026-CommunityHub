using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.services;

namespace ChatModule.ViewModels;

public class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IAuthenticationService _authentificationService;

    private string email = string.Empty;
    public string Email
    {
        get => email;
        set => Set(ref email, value);
    }

    private string newPassword = string.Empty;
    public string NewPassword
    {
        get => newPassword;
        set => Set(ref newPassword, value);
    }

    private string? errorMessage;
    public string? ErrorMessage
    {
        get => errorMessage;
        set => Set(ref errorMessage, value);
    }

    private string? successMessage;
    public string? SuccessMessage
    {
        get => successMessage;
        set => Set(ref successMessage, value);
    }

    public RelayCommand SubmitCommand { get; }
    public RelayCommand BackToLoginCommand { get; }

    public event Action? NavigateToLoginRequested;

    public ForgotPasswordViewModel(IAuthenticationService authentificationService)
    {
        this._authentificationService = authentificationService ?? throw new ArgumentNullException(nameof(authentificationService));
        SubmitCommand = new RelayCommand(SubmitAsync);
        BackToLoginCommand = new RelayCommand(BackToLoginAsync);
    }

    private async Task SubmitAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await _authentificationService.ChangePasswordAsync(Email, NewPassword);
            SuccessMessage = "Password updated";
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    private Task BackToLoginAsync()
    {
        NavigateToLoginRequested?.Invoke();
        return Task.CompletedTask;
    }
}
