using ChatAndEvents.Data.CommunityHub.Models;
using Microsoft.AspNetCore.Http;

namespace ChatAndEvents.Web.Models;

public class MatchmakingEditViewModel
{
    public static readonly IReadOnlyList<string> GenderOptions =
    [
        "Woman",
        "Man",
        "Non-binary",
        "Other",
        "Prefer not to say",
    ];

    public static readonly IReadOnlyList<string> PreferredGenderOptions =
    [
        "Women",
        "Men",
        "Non-binary people",
        "Everyone",
    ];

    public static readonly IReadOnlyList<string> LoverTypeOptions =
    [
        "Creative Partner",
        "Adventure Match",
        "Cozy Match",
        "Event Buddy",
        "Slow Burn",
        "High Energy",
        "Calm Match",
        "Community Builder",
    ];

    public static readonly IReadOnlyList<string> InterestOptions =
    [
        "Tech",
        "Music",
        "Gaming",
        "Books",
        "Coffee",
        "Design",
        "Sports",
        "Hiking",
        "Art",
        "Volunteering",
        "Board games",
        "Festivals",
        "Photography",
        "Hackathons",
        "Travel",
        "Fitness",
    ];

    public static readonly IReadOnlyList<string> LocationOptions =
    [
        "Alba Iulia",
        "Arad",
        "Bacau",
        "Baia Mare",
        "Bistrita",
        "Brasov",
        "Bucuresti",
        "Cluj-Napoca",
        "Constanta",
        "Craiova",
        "Deva",
        "Galati",
        "Iasi",
        "Oradea",
        "Pitesti",
        "Ploiesti",
        "Sibiu",
        "Suceava",
        "Targu Mures",
        "Timisoara",
    ];

    public string DisplayName { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string PreferredGenders { get; set; } = string.Empty;

    public List<string> PreferredGenderSelections { get; set; } = [];

    public string Location { get; set; } = string.Empty;

    public string DatingBio { get; set; } = string.Empty;

    public string Interests { get; set; } = string.Empty;

    public List<string> SelectedInterests { get; set; } = [];

    public string PhotoUrls { get; set; } = string.Empty;

    public string ExistingPhotoUrls { get; set; } = string.Empty;

    public List<IFormFile> PhotoFiles { get; set; } = [];

    public int MinPreferredAge { get; set; } = 18;

    public int MaxPreferredAge { get; set; } = 35;

    public int MaxDistanceKm { get; set; } = 50;

    public string LoverType { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public static MatchmakingEditViewModel FromProfile(DatingProfile? profile, string fallbackName)
    {
        if (profile == null)
        {
            return new MatchmakingEditViewModel
            {
                DisplayName = fallbackName,
                PreferredGenders = "Everyone",
                PreferredGenderSelections = ["Everyone"],
            };
        }

        return new MatchmakingEditViewModel
        {
            DisplayName = profile.DisplayName,
            Gender = profile.Gender,
            PreferredGenders = profile.PreferredGenders,
            Location = profile.Location,
            DatingBio = profile.DatingBio,
            Interests = string.Join(", ", profile.Interests.Select(interest => interest.Name)),
            SelectedInterests = profile.Interests.Select(interest => interest.Name).ToList(),
            PhotoUrls = string.Join(Environment.NewLine, profile.Photos.OrderBy(photo => photo.SortOrder).Select(photo => photo.Url)),
            ExistingPhotoUrls = string.Join(Environment.NewLine, profile.Photos.OrderBy(photo => photo.SortOrder).Select(photo => photo.Url)),
            MinPreferredAge = profile.MinPreferredAge,
            MaxPreferredAge = profile.MaxPreferredAge,
            MaxDistanceKm = profile.MaxDistanceKm,
            LoverType = profile.LoverType,
            IsEnabled = profile.IsEnabled,
            PreferredGenderSelections = profile.PreferredGenders
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList(),
        };
    }
}
