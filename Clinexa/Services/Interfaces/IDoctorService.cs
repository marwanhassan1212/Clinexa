using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<Doctor?> GetByIdAsync(int id);

        Task<List<Doctor>> GetAllAsync();

        Task<bool> CreateAsync(Doctor doctor);

        Task<bool> UpdateAsync(Doctor doctor);

        Task<bool> DeactivateAsync(int id);
    }
}
