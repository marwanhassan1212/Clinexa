namespace Clinexa.Models.ViewModels.Role
{
    public class RoleDetailsViewModel
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UsersCount { get; set; }
    }
}
