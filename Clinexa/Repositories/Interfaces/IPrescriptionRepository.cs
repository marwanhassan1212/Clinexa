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
        Task<List<MedicalRecord>> SearchMedicalRecordsAsync(string? search, int take = 10);

        Task<bool> IsMedicalRecordAppointmentCompletedAsync(int medicalRecordId);
        Task<(List<Prescription> Prescriptions, int TotalCount)> FilterAsync(string? search,
        DateTime? dateFrom, DateTime? dateTo, int? medicalRecordId, string sortBy, string sortDirection,
                       int page, int pageSize);
        Task<List<MedicalRecord>> GetMedicalRecordsAsync();

        Task AddAsync(Prescription prescription);

        void Update(Prescription prescription);

        Task SaveChangesAsync();
    }
}
