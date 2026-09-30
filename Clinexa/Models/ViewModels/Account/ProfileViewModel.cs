namespace Clinexa.Models.ViewModels.Account
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; }
    }
}