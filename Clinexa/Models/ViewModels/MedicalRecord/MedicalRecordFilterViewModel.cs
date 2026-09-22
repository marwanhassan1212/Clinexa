namespace Clinexa.Models.ViewModels.MedicalRecord
{
    public class MedicalRecordFilterViewModel
    {
        public string? Search { get; set; }

        public int? PatientId { get; set; }

        public int? DoctorId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string SortBy { get; set; } = "Date";

        public string SortDirection { get; set; } = "Desc";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        public List<Entities.MedicalRecord> MedicalRecords { get; set; }
            = new();

        public List<Entities.Patient> Patients { get; set; }
            = new();

        public List<Entities.Doctor> Doctors { get; set; }
            = new();
    }
}
