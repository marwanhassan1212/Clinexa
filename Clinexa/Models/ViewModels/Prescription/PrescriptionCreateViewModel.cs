using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Prescription
{
    public class PrescriptionCreateViewModel
    {
        [Required]
        [Display(Name = "Medical Record")]
        public int MedicalRecordId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PrescriptionDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

    }
}
