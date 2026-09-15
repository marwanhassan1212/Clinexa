using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Speciality
{
    public class SpecialityEditViewModel
    {
        [Required]
        public int SpecialityId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
