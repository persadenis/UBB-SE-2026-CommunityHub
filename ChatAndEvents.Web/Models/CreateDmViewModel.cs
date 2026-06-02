using System.ComponentModel.DataAnnotations;

namespace ChatAndEvents.Web.Models
{
    public class CreateDmViewModel
    {
        [Required]
        public Guid TargetUserId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}