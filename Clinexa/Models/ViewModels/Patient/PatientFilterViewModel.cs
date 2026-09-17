using Clinexa.Enums;
using Clinexa.Models.Entities;

namespace Clinexa.Models.ViewModels.Patient
{
    public class PatientFilterViewModel
    {
        public string? Search { get; set; }

        public Gender? Gender { get; set; }

        public string? BloodType { get; set; }

        public bool? IsActive { get; set; }

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        public DateTime? RegisteredFrom { get; set; }

        public DateTime? RegisteredTo { get; set; }

        public string SortBy { get; set; } = "CreatedAt";

        public string SortDirection { get; set; } = "desc";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);

        public List<Clinexa.Models.Entities.Patient> Patients { get; set; } = new();
    }
}