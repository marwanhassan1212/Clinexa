using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.MedicalRecord
{
    public class MedicalRecordCreateViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int AppointmentId { get; set; }
    }
}
