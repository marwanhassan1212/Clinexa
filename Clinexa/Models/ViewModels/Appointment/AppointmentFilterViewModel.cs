using Clinexa.Enums;

namespace Clinexa.Models.ViewModels.Appointment
{
    public class AppointmentFilterViewModel
    {
        public string? Search { get; set; }

        public int? DoctorId { get; set; }

        public int? PatientId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public AppointmentStatus? Status { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public List<Entities.Appointment> Appointments { get; set; } = new();

        public List<Entities.Doctor> Doctors { get; set; } = new();

        public List<Entities.Patient> Patients { get; set; } = new();

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
