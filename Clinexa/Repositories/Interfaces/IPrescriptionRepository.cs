using Clinexa.Models.Entities;
using System.Threading.Tasks;

namespace Clinexa.Repositories.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<Prescription?> GetByIdAsync(int id);

        Task<List<Prescription>> GetAllAsync();

        Task<Prescription?> GetByMedicalRecordIdAsync(
            int medicalRecordId);

        Task<bool> MedicalRecordExistsAsync(
            int medicalRecordId);

        Task<bool> ExistsForMedicalRecordAsync(
            int medicalRecordId,
            int? excludedPrescriptionId = null);

        Task AddAsync(Prescription prescription);

        void Update(Prescription prescription);

        Task SaveChangesAsync();
    }
}
