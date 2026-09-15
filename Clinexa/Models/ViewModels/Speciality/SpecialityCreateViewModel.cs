using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Speciality
{
    public class SpecialityCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
