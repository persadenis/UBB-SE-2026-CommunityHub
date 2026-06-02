using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.achievementServices;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using ChatAndEvents.Data.EventsData.Services.userServices;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Events_GSS.ViewModels;

public class ReputationViewModel : INotifyPropertyChanged
{
    private readonly IUserService _userService;
    private readonly IReputationService _reputationService;
    private readonly IAchievementService _achievementService;

    private string _userName = string.Empty;
    public string UserName
    {
        get => _userName;
        private set { _userName = value; OnPropertyChanged(); }
    }

    private int _reputationPoints;
    public int ReputationPoints
    {
        get => _reputationPoints;
        private set { _reputationPoints = value; OnPropertyChanged(); }
    }

    private string _currentTier = "Newcomer";
    public string CurrentTier
    {
        get => _currentTier;
        private set { _currentTier = value; OnPropertyChanged(); }
    }

    private ObservableCollection<Achievement> _achievements = new();
    public ObservableCollection<Achievement> Achievements
    {
        get => _achievements;
        private set { _achievements = value; OnPropertyChanged(); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set { _errorMessage = value; OnPropertyChanged(); }
    }

    public ReputationViewModel(IUserService userService, IReputationService reputationService, IAchievementService achievementService)
    {
        _userService = userService;
        _reputationService = reputationService;
        _achievementService = achievementService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var user = await _userService.GetCurrentUser();
            var reputationScore = await _reputationService.GetReputationScoreAsync(user.UserId);

            UserName = user.Name;
            ReputationPoints = reputationScore.ReputationPoints;
            CurrentTier = reputationScore.Tier;

            Achievements = new ObservableCollection<Achievement>(
                await _achievementService.GetUserAchievementsAsync(user.UserId));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load reputation: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
