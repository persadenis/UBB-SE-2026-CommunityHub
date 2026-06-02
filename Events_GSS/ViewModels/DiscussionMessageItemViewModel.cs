using System;
using System.Collections.Generic;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.ViewModelsCore;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Events_GSS.ViewModels;

public partial class DiscussionMessageItemViewModel : ObservableObject
{
    public DiscussionMessage Model { get; }
    private readonly Guid _currentUserId;
    private readonly bool _isCurrentUserAdmin;

    public DiscussionMessageItemViewModel(
        DiscussionMessage model,
        Guid currentUserId,
        bool isCurrentUserAdmin)
    {
        Model = model;
        _currentUserId = currentUserId;
        _isCurrentUserAdmin = isCurrentUserAdmin;
    }

    // ── Model pass-throughs ───────────────────────────────────────────────────

    public int Id => Model.Id;
    public string? Message => Model.Message;
    public string? MediaPath => Model.MediaPath;
    public DateTime Date => Model.DateCreated;
    public bool IsEdited => Model.IsEdited;
    public bool CanDelete => Model.CanDelete;
    public User? Author => Model.Author;
    public DiscussionMessage? ReplyTo => Model.ReplyTo;

    // ── Delegated to core ─────────────────────────────────────────────────────

    public List<ReactionGroup> ReactionGroups =>
        DiscussionMessageItemViewModelCore.BuildReactionGroups(Model.Reactions, _currentUserId);

    public bool HasReactions =>
        DiscussionMessageItemViewModelCore.HasReactions(Model.Reactions);

    public string? CurrentUserEmoji =>
        DiscussionMessageItemViewModelCore.CurrentUserEmoji(Model.Reactions, _currentUserId);

    public bool ShowMuteButton =>
        DiscussionMessageItemViewModelCore.ShowMuteButton(
            _isCurrentUserAdmin, Model.Author?.UserId, _currentUserId);

    public List<MessageSegment> MessageSegments =>
        DiscussionMessageItemViewModelCore.ParseMessageIntoSegments(Message);

    public bool HasMessageText =>
        DiscussionMessageItemViewModelCore.HasMessageText(Message);

    // ── UI-only state ─────────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isOriginalDeleted;
}