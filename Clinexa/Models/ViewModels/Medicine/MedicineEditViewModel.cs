using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Medicine
{
    public class MedicineEditViewModel
    {
        [Required]
        public int MedicineId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(300)]
        public string? GenericName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
