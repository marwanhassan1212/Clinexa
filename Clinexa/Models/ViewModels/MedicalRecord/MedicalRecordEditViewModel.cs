using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.MedicalRecord
{
    public class MedicalRecordEditViewModel
    {
        [Required]
        public int MedicalRecordId { get; set; }

        [StringLength(1000)]
        [Display(Name = "Symptoms")]
        public string? Symptoms { get; set; }

        [StringLength(1000)]
        [Display(Name = "Diagnosis")]
        public string? Diagnosis { get; set; }

        [StringLength(2000)]
        [Display(Name = "Treatment")]
        public string? Treatment { get; set; }

        [StringLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Follow-up Date")]
        public DateTime? FollowUpDate { get; set; }
    }
}
