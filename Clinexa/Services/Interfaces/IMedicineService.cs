using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IMedicineService
    {
        Task<Medicine?> GetByIdAsync(int id);

        Task<List<Medicine>> GetAllAsync();

        Task<List<Medicine>> GetActiveAsync();

        Task<bool> CreateAsync(Medicine medicine);

        Task<(List<Medicine> Medicines, int TotalCount)> FilterAsync(string? search, bool? isActive,
                    int page, int pageSize);
        Task<bool> UpdateAsync(Medicine medicine);

        Task<bool> DeactivateAsync(int id);
        Task<bool> ActivateAsync(int id);

    }
}
