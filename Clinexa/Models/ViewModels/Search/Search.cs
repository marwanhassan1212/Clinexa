namespace Clinexa.Models.ViewModels.Search
{
    public class SearchResultViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public List<PatientSearchResultViewModel> Patients { get; set; } = new();

        public List<DoctorSearchResultViewModel> Doctors { get; set; } = new();

        public List<AppointmentSearchResultViewModel> Appointments { get; set; } = new();
    }


    public class PatientSearchResultViewModel
    {
        public int PatientId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }


    public class DoctorSearchResultViewModel
    {
        public int DoctorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? SpecialityName { get; set; }
    }


    public class AppointmentSearchResultViewModel
    {
        public int AppointmentId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
