namespace Clinexa.Models.ViewModels.Prescription
{
    public class PrescriptionFilterViewModel
    {
        public string? Search { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int? MedicalRecordId { get; set; }

        public string SortBy { get; set; } = "Date";

        public string SortDirection { get; set; } = "Desc";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        public List<Entities.Prescription> Prescriptions { get; set; } = new();

        public List<Entities.MedicalRecord> MedicalRecords { get; set; } = new();
    }
}
