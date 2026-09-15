using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IDoctorScheduleService
    {
        Task<DoctorSchedule?> GetByIdAsync(int id);

        Task<List<DoctorSchedule>> GetAllAsync();

        Task<List<DoctorSchedule>> GetByDoctorIdAsync(int doctorId);

        Task<bool> CreateAsync(DoctorSchedule schedule);

        Task<bool> UpdateAsync(DoctorSchedule schedule);

        Task<bool> DeactivateAsync(int id);
    }
}
