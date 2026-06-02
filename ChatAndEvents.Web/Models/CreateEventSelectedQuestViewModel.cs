namespace ChatAndEvents.Web.Models;

public class CreateEventSelectedQuestViewModel
{
    public string Key { get; set; } = Guid.NewGuid().ToString("N");

    public int? PresetQuestId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Difficulty { get; set; } = 3;

    public bool IsCustom => !PresetQuestId.HasValue;
}
