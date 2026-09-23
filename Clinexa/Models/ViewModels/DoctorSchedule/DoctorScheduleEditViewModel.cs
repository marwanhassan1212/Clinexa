using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.DoctorSchedule
{
    public class DoctorScheduleEditViewModel
    {
        [Required]
        public int DoctorScheduleId { get; set; }

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
