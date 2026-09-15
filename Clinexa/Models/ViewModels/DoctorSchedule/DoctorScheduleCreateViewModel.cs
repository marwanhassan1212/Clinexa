using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.DoctorSchedule
{
    public class DoctorScheduleCreateViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
