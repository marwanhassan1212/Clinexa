using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IPrescriptionItemService
    {
        Task<PrescriptionItem?> GetByIdAsync(int id);

        Task<List<PrescriptionItem>> GetAllAsync();

        Task<List<PrescriptionItem>> GetByPrescriptionIdAsync(
            int prescriptionId);

        Task<bool> CreateAsync(
            PrescriptionItem prescriptionItem);

        Task<bool> UpdateAsync(
            PrescriptionItem prescriptionItem);
    }
}
