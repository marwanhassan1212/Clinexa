using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Appointment
{
    public class AppointmentCreateViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        public string? Reason { get; set; }

        public string? Notes { get; set; }

        public List<Entities.Patient> Patients { get; set; } = new();
        public List<Entities.Doctor> Doctors { get; set; } = new();

        public List<TimeSpan> AvailableSlots { get; set; } = new();


    }
}
