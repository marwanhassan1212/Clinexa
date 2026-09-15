using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IMedicineRepository
    {
        Task<Medicine?> GetByIdAsync(int id);

        Task<List<Medicine>> GetAllAsync();

        Task<List<Medicine>> GetActiveAsync();

        Task<bool> ExistsByNameAsync(
            string name,
            int? excludedMedicineId = null);

        Task AddAsync(Medicine medicine);

        void Update(Medicine medicine);

        Task SaveChangesAsync();
    }
}
