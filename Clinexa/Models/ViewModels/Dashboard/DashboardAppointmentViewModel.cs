namespace Clinexa.Models.ViewModels.Dashboard
{
    public class DashboardAppointmentViewModel
    {
        public int AppointmentId { get; set; }

        public string PatientName { get; set; } = null!;

        public string DoctorName { get; set; } = null!;

        public TimeSpan StartTime { get; set; }

        public Clinexa.Enums.AppointmentStatus Status { get; set; }
    }
}
