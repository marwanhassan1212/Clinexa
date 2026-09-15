using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(int id);
        Task<List<Doctor>> GetAllAsync();
        Task<bool> ExistsByUserId(int id);
        Task<bool> ExistBySpecialityId(int id);
        Task AddAsync(Doctor doctor);
        void UpdateAsync(Doctor doctor);
        Task SaveChangesAsync();

    }
}
