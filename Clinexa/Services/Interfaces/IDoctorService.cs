using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<Doctor?> GetByIdAsync(int id);

        Task<List<Doctor>> GetAllAsync();

        Task<bool> CreateAsync(Doctor doctor);

        Task<bool> ActivateAsync(int id);

        Task<(List<Doctor> Doctors, int TotalCount)> FilterAsync(string? search, int? specialityId,
                         bool? isActive, int page, int pageSize);
        Task<bool> UpdateAsync(Doctor doctor);

        Task<bool> DeactivateAsync(int id);
    }
}
