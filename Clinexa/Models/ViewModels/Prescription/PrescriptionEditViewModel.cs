using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Prescription
{
    public class PrescriptionEditViewModel
    {
        [Required]
        public int PrescriptionId { get; set; }

        [Required]
        public DateTime PrescriptionDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public int MedicalRecordId { get; set; }
    }
}
