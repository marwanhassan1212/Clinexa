using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.User
{
    public class UserCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [StringLength(255, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a role.")]
        public int RoleId { get; set; }
    }
}
