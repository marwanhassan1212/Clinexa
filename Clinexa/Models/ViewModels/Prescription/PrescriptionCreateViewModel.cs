using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Prescription
{
    public class PrescriptionCreateViewModel
    {
        [Required]
        public DateTime PrescriptionDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public int MedicalRecordId { get; set; }
    }
}
