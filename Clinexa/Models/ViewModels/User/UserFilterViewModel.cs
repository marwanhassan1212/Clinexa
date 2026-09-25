namespace Clinexa.Models.ViewModels.User
{
    public class UserFilterViewModel
    {
        public string? Search { get; set; }

        public int? RoleId { get; set; }

        public bool? IsActive { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public List<Entities.User> Users { get; set; } = new();

        public List<Entities.Role> Roles { get; set; } = new();

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(
                (double)TotalCount / PageSize);

        public Dictionary<int, string?> RoleNames { get; set; } = new();
    }
}
