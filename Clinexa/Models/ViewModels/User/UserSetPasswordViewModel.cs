using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.User
{
    public class UserSetPasswordViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
    }
}
