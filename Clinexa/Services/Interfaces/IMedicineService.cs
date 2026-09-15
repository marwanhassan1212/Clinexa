using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IMedicineService
    {
        Task<Medicine?> GetByIdAsync(int id);

        Task<List<Medicine>> GetAllAsync();

        Task<List<Medicine>> GetActiveAsync();

        Task<bool> CreateAsync(Medicine medicine);

        Task<bool> UpdateAsync(Medicine medicine);

        Task<bool> DeactivateAsync(int id);
    }
}
