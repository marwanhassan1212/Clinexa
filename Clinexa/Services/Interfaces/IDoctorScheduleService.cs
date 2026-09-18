using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IDoctorScheduleService
    {
        Task<DoctorSchedule?> GetByIdAsync(int id);

        Task<List<DoctorSchedule>> GetAllAsync();

        Task<List<DoctorSchedule>> GetByDoctorIdAsync(int doctorId);

        Task<(List<DoctorSchedule> Schedules, int TotalCount)> FilterAsync(
                string? search,
                int? doctorId,
                DayOfWeek? dayOfWeek,
                bool? isAvailable,
                int page,
                int pageSize);

        Task<bool> CreateAsync(DoctorSchedule schedule);

        Task<bool> UpdateAsync(DoctorSchedule schedule);

        Task<bool> DeactivateAsync(int id);

        Task<bool> ActivateAsync(int id);
    }
}
