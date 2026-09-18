using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Appointment
{
    public class AppointmentEditViewModel
    {
        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}
