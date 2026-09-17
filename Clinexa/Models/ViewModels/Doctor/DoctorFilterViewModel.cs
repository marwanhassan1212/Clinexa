namespace Clinexa.Models.ViewModels.Doctor
{
    public class DoctorFilterViewModel
    {
        public string? Search { get; set; }

        public int? SpecialityId { get; set; }

        public bool? IsActive { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public List<Entities.Doctor> Doctors { get; set; } = new();

        public List<Entities.Speciality> Specialities { get; set; } = new();

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}