using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface ISpecialityService
    {
        Task<Speciality?> GetByIdAsync(int id);
        Task<List<Speciality>> GetAllAsync();
        Task<bool> CreateAsync(Speciality speciality);
        Task<bool> UpdateAsync(Speciality speciality);
        Task<bool> DeactivateAsync(int id);
    }
}
