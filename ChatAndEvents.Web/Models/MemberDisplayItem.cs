using System;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Web.Models;

public class MemberDisplayItem
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
    public ParticipantRole Role { get; set; }
    public bool HasTimeout { get; set; }
    public DateTime? TimeoutUntil { get; set; }

    public string RoleLabel => Role switch
    {
        ParticipantRole.Admin => "Admin",
        ParticipantRole.Banned => "Banned",
        _ => "Member",
    };

    public string StatusLabel => Status switch
    {
        UserStatus.Online => "Online",
        UserStatus.Busy => "Busy",
        _ => "Offline",
    };

    public string StatusCssClass => Status switch
    {
        UserStatus.Online => "bg-success",
        UserStatus.Busy => "bg-warning",
        _ => "bg-secondary",
    };

    public bool IsMemberRole => Role == ParticipantRole.Member;
    public bool IsAdminRole => Role == ParticipantRole.Admin;

    public string? TimeoutRemainingLabel
    {
        get
        {
            if (!TimeoutUntil.HasValue || TimeoutUntil.Value <= DateTime.UtcNow)
            {
                return null;
            }

            var remaining = TimeoutUntil.Value - DateTime.UtcNow;
            if (remaining.TotalDays >= 1)
            {
                return $"Timeout: {(int)remaining.TotalDays}d {remaining.Hours}h left";
            }

            if (remaining.TotalHours >= 1)
            {
                return $"Timeout: {(int)remaining.TotalHours}h {remaining.Minutes}m left";
            }

            return $"Timeout: {remaining.Minutes}m left";
        }
    }
}
