using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(int id);
        Task<List<Doctor>> GetAllAsync();
        Task<bool> ExistsByUserId(int id);
        Task<bool> ExistBySpecialityId(int id);
        Task<List<User>> GetAvailableUsersAsync();

        Task AddAsync(Doctor doctor);

        Task<(List<Doctor> Doctors, int TotalCount)> FilterAsync(string? search, int? specialityId,
                        bool? isActive, int page, int pageSize);
        void UpdateAsync(Doctor doctor);
        Task SaveChangesAsync();

    }
}
