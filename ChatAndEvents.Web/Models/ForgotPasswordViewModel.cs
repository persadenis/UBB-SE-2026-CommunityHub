using System.ComponentModel.DataAnnotations;

namespace ChatAndEvents.Web.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}