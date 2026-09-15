using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IDoctorScheduleRepository
    {
        Task<DoctorSchedule?> GetByIdAsync(int id);

        Task<List<DoctorSchedule>> GetAllAsync();

        Task<List<DoctorSchedule>> GetByDoctorIdAsync(int doctorId);

        Task<bool> ExistsAsync(
            int doctorId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime);

        Task<bool> DoctorExistsAsync(int doctorId);

        Task AddAsync(DoctorSchedule schedule);

        void Update(DoctorSchedule schedule);

        Task SaveChangesAsync();
    }
}
