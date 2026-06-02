// <copyright file="AnnouncementItemViewModelCore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.ViewModelsCore;

using ChatAndEvents.Data.EventsData.Models;

/// <summary>
/// Represents the core logic for an announcement item view model, providing properties and methods.
/// </summary>
public sealed class AnnouncementItemViewModelCore
{
    private readonly Announcement _announcementModel;
    private readonly Guid _currentUserId;

    public AnnouncementItemViewModelCore(Announcement announcementModel, Guid currentUserId)
    {
        this._announcementModel = announcementModel;
        this._currentUserId = currentUserId;
    }

    public string PreviewText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_announcementModel.Message))
            {
                return string.Empty;
            }

            var firstLine = _announcementModel.Message.Split('\n', 2)[0];
            return firstLine.Length > 120 ? firstLine[..120] + "…" : firstLine;
        }
    }

    public bool HasFullContent =>
        _announcementModel.Message.Contains('\n') || _announcementModel.Message.Length > 120;

    public List<ReactionGroup> ReactionGroups =>
        _announcementModel.Reactions
            .GroupBy(r => r.Emoji)
            .Select(group => new ReactionGroup
            {
                Emoji = group.Key,
                Count = group.Count(),
                CurrentUserReacted =
                    group.Any(r => r.Author.Id == _currentUserId),
            })
            .ToList();

    public string? CurrentUserEmoji =>
        _announcementModel.Reactions
            .FirstOrDefault(r => r.Author.Id == _currentUserId)?
            .Emoji;
}
