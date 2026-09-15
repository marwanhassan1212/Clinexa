using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Role
{
    public class RoleCreateViewModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [StringLength(250)]
        public string? Description { get; set; }
    }
}

