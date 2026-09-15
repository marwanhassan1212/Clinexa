using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.MedicalRecord
{
    public class MedicalRecordEditViewModel
    {
        [Required]
        public int MedicalRecordId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int AppointmentId { get; set; }
    }
}
