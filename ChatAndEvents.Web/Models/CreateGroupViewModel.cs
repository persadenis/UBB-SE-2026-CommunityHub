using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatAndEvents.Web.Models
{
    public class CreateGroupViewModel
    {
        [Required(ErrorMessage = "Group name is required.")]
        [StringLength(100, MinimumLength = 1)]
        public string GroupName { get; set; } = string.Empty;

        public string? IconUrl { get; set; }

        // Comma-separated user IDs submitted with the form
        public string SelectedMemberIds { get; set; } = string.Empty;

        // For displaying selected members on the page
        public List<UserDto> SelectedMembers { get; set; } = new();

        // For displaying search results
        public List<UserDto> SearchResults { get; set; } = new();

        public string? MemberSearchQuery { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}