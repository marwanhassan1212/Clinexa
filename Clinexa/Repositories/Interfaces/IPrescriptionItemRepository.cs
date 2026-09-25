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

        Task<bool> ActiveMedicineExistsAsync(int medicineId);

        Task<bool> ExistsForPrescriptionAsync(
                int prescriptionId,
                int medicineId,
                int? excludedPrescriptionItemId = null);

        Task<bool> BelongsToDoctorAsync(int prescriptionItemId, int doctorUserId);
        Task<bool> PrescriptionBelongsToDoctorAsync(int prescriptionId, int doctorUserId);
        Task AddAsync(
            PrescriptionItem prescriptionItem);

        void Update(
            PrescriptionItem prescriptionItem);

        Task SaveChangesAsync();
    }
}
