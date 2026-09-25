using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Clinexa.Services.Implementations
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository prescriptionRepository;
        private readonly UserManager<User> userManager;

        public PrescriptionService(
            IPrescriptionRepository prescriptionRepository,
            UserManager<User> userManager)
        {
            this.prescriptionRepository = prescriptionRepository;
            this.userManager = userManager;
        }

        public async Task<bool> CreateAsync(
            Prescription prescription)
        {
            bool medicalRecordExists =
                await prescriptionRepository
                    .MedicalRecordExistsAsync(
                        prescription.MedicalRecordId);

            if (!medicalRecordExists)
            {
                return false;
            }

            bool appointmentCompleted =
                 await prescriptionRepository
                     .IsMedicalRecordAppointmentCompletedAsync(
                         prescription.MedicalRecordId);

            if (!appointmentCompleted)
            {
                return false;
            }

            bool prescriptionExists =
                await prescriptionRepository
                    .ExistsForMedicalRecordAsync(
                        prescription.MedicalRecordId);

            if (prescriptionExists)
            {
                return false;
            }

            if (prescription.PrescriptionDate >
                DateTime.Now)
            {
                return false;
            }

            await prescriptionRepository
                .AddAsync(prescription);

            await prescriptionRepository
                .SaveChangesAsync();

            return true;
        }

        public async Task<List<Prescription>> GetAllAsync()
        {
            return await prescriptionRepository
                .GetAllAsync();
        }

        public async Task<Prescription?> GetByIdAsync(
            int id)
        {
            return await prescriptionRepository
                .GetByIdAsync(id);
        }

        public Task<Prescription?> GetByMedicalRecordIdAsync(
            int medicalRecordId)
        {
            return prescriptionRepository
                .GetByMedicalRecordIdAsync(
                    medicalRecordId);
        }

        public async Task<bool> UpdateAsync(
            Prescription prescription)
        {
            var prescriptionExists =
                await prescriptionRepository
                    .GetByIdAsync(
                        prescription.PrescriptionId);

            if (prescriptionExists == null)
            {
                return false;
            }

            bool medicalRecordExists =
                await prescriptionRepository
                    .MedicalRecordExistsAsync(
                        prescription.MedicalRecordId);

            if (!medicalRecordExists)
            {
                return false;
            }

            bool duplicate =
                await prescriptionRepository
                    .ExistsForMedicalRecordAsync(
                        prescription.MedicalRecordId,
                        prescription.PrescriptionId);

            if (duplicate)
            {
                return false;
            }

            if (prescription.PrescriptionDate >
                DateTime.UtcNow)
            {
                return false;
            }

            prescriptionRepository
                .Update(prescription);

            await prescriptionRepository
                .SaveChangesAsync();

            return true;
        }

        // =========================================================
        // Filtering
        // =========================================================

        public async Task<(
            List<Prescription> Prescriptions,
            int TotalCount)> FilterAsync(
                string? search,
                DateTime? dateFrom,
                DateTime? dateTo,
                int? medicalRecordId,
                string sortBy,
                string sortDirection,
                int page,
                int pageSize,
                int? doctorUserId = null)
        {
            return await prescriptionRepository
                .FilterAsync(
                    search,
                    dateFrom,
                    dateTo,
                    medicalRecordId,
                    sortBy,
                    sortDirection,
                    page,
                    pageSize,
                    doctorUserId);
        }

        // =========================================================
        // Medical Records
        // =========================================================

        public async Task<List<MedicalRecord>>
            GetMedicalRecordsAsync(
                int? doctorUserId = null)
        {
            return await prescriptionRepository
                .GetMedicalRecordsAsync(
                    doctorUserId);
        }

        public async Task<List<MedicalRecord>>
            SearchMedicalRecordsAsync(
                string? search,
                int take = 10,
                int? doctorUserId = null)
        {
            return await prescriptionRepository
                .SearchMedicalRecordsAsync(
                    search,
                    take,
                    doctorUserId);
        }

        // =========================================================
        // Data-Level Authorization
        // =========================================================

        public async Task<bool> CanAccessAsync(
            int prescriptionId,
            int currentUserId)
        {
            var user =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (user == null)
            {
                return false;
            }

            // Admin can access all prescriptions.
            if (await userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                return true;
            }

            // Doctor can access only prescriptions
            // belonging to their own medical records.
            return await prescriptionRepository
                .BelongsToDoctorAsync(
                    prescriptionId,
                    currentUserId);
        }

        public async Task<bool> CanAccessMedicalRecordAsync(
            int medicalRecordId,
            int currentUserId)
        {
            var user =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (user == null)
            {
                return false;
            }

            // Admin can access all medical records.
            if (await userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                return true;
            }

            // Doctor can access only their own medical records.
            return await prescriptionRepository
                .MedicalRecordBelongsToDoctorAsync(
                    medicalRecordId,
                    currentUserId);
        }
    }
}