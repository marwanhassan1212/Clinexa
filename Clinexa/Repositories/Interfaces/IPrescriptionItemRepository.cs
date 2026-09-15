using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IPrescriptionItemRepository
    {
        Task<PrescriptionItem?> GetByIdAsync(int id);

        Task<List<PrescriptionItem>> GetAllAsync();

        Task<List<PrescriptionItem>> GetByPrescriptionIdAsync(
            int prescriptionId);

        Task<bool> PrescriptionExistsAsync(
            int prescriptionId);

        Task<bool> MedicineExistsAsync(
            int medicineId);

        Task AddAsync(
            PrescriptionItem prescriptionItem);

        void Update(
            PrescriptionItem prescriptionItem);

        Task SaveChangesAsync();
    }
}
