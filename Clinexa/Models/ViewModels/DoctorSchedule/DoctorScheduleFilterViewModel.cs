namespace Clinexa.Models.ViewModels.DoctorSchedule
{
    public class DoctorScheduleFilterViewModel
    {
        public string? Search { get; set; }

        public int? DoctorId { get; set; }

        public DayOfWeek? DayOfWeek { get; set; }

        public bool? IsAvailable { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public List<Entities.DoctorSchedule> Schedules { get; set; } = new();

        public List<Entities.Doctor> Doctors { get; set; } = new();

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
