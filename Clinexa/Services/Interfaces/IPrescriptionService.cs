using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IPrescriptionService
    {
        Task<Prescription?> GetByIdAsync(int id);

        Task<List<Prescription>> GetAllAsync();

        Task<Prescription?> GetByMedicalRecordIdAsync(
            int medicalRecordId);

        Task<bool> CreateAsync(
            Prescription prescription);

        Task<bool> UpdateAsync(
            Prescription prescription);

        Task<bool> CanAccessMedicalRecordAsync(
            int medicalRecordId,
            int currentUserId);

        Task<bool> CanAccessAsync(
            int prescriptionId,
            int currentUserId);

        Task<List<MedicalRecord>> SearchMedicalRecordsAsync(
            string? search,
            int take = 10,
            int? doctorUserId = null);

        Task<(List<Prescription> Prescriptions, int TotalCount)> FilterAsync(
            string? search,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? medicalRecordId,
            string sortBy,
            string sortDirection,
            int page,
            int pageSize,
            int? doctorUserId = null);

        Task<List<MedicalRecord>> GetMedicalRecordsAsync(
            int? doctorUserId = null);
    }
}