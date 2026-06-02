using System;
using System.ComponentModel.DataAnnotations;

namespace ChatAndEvents.Web.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(16, MinimumLength = 5, ErrorMessage = "Username must be 5–16 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birthday is required.")]
        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }
    }
}