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
        Task<bool> IsMedicalRecordAppointmentCompletedAsync(
    int medicalRecordId);
        Task<bool> ExistsForMedicalRecordAsync(
            int medicalRecordId,
            int? excludedPrescriptionId = null);
        Task<List<MedicalRecord>> SearchMedicalRecordsAsync(string? search, int take = 10, int? doctorUserId = null);
        Task<(List<Prescription> Prescriptions, int TotalCount)> FilterAsync(string? search, DateTime? dateFrom,
            DateTime? dateTo, int? medicalRecordId, string sortBy, string sortDirection, int page,
            int pageSize, int? doctorUserId = null);
      Task<List<MedicalRecord>> GetMedicalRecordsAsync(
            int? doctorUserId = null);

        Task<bool> BelongsToDoctorAsync(int prescriptionId, int doctorUserId);
        Task<bool> MedicalRecordBelongsToDoctorAsync(int medicalRecordId, int doctorUserId);
        Task AddAsync(Prescription prescription);

        void Update(Prescription prescription);

        Task SaveChangesAsync();
    }
}
