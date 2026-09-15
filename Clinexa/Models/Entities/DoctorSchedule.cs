namespace Clinexa.Models.Entities
{
    public class DoctorSchedule
    {
        public int DoctorScheduleId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public int DoctorId { get; set; }

    }
}
