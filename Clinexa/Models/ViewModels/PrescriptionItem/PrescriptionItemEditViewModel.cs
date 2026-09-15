using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.PrescriptionItem
{
    public class PrescriptionItemEditViewModel
    {
        [Required]
        public int PrescriptionItemId { get; set; }

        [Required]
        public int PrescriptionId { get; set; }

        [Required]
        public int MedicineId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Dosage { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Frequency { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string Duration { get; set; } = null!;

        [MaxLength(500)]
        public string? Instructions { get; set; }
    }
}
