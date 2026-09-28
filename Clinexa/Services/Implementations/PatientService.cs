
using System.Security.Claims;
using System.Text.Json;
using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Clinexa.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository patientRepository;
        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;

        public PatientService(
            IPatientRepository patientRepository,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            this.patientRepository = patientRepository;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }

        // =========================================================
        // Create
        // =========================================================

        public async Task<bool> CreateAsync(Patient patient)
        {
            bool phoneExist =
                await patientRepository
                    .ExistsByPhoneAsync(
                        patient.PhoneNumber);

            if (phoneExist)
            {
                return false;
            }

            patient.CreatedAt = DateTime.UtcNow;
            patient.IsActive = true;

            await patientRepository.AddAsync(patient);
            await patientRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
                {
                    patient.PatientId,
                    patient.PhoneNumber,
                    patient.IsActive,
                    patient.CreatedAt
                });

                await auditLogService.LogAsync(
                    "Create",
                    "Patient",
                    patient.PatientId,
                    userId.Value,
                    newValues: newValues,
                    ipAddress: GetIpAddress());
            }

            return true;
        }

        // =========================================================
        // Deactivate
        // =========================================================

        public async Task<bool> DeactivateAsync(int id)
        {
            var patientExists =
                await patientRepository.GetByIdAsync(id);

            if (patientExists == null)
            {
                return false;
            }

            var oldValues =
                SerializePatient(patientExists);

            patientExists.IsActive = false;

            patientRepository.Update(patientExists);

            await patientRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Deactivate",
                patientExists,
                oldValues);

            return true;
        }

        // =========================================================
        // Get
        // =========================================================

        public async Task<List<Patient>> GetAllAsync()
        {
            return await patientRepository.GetAllAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await patientRepository.GetByIdAsync(id);
        }

        public async Task<List<Patient>> SearchAsync(
            string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await patientRepository.GetAllAsync();
            }

            return await patientRepository.SearchAsync(
                searchTerm);
        }

        // =========================================================
        // Update
        // =========================================================

        public async Task<bool> UpdateAsync(Patient patient)
        {
            var patientExists =
                await patientRepository
                    .GetByIdAsync(
                        patient.PatientId);

            if (patientExists == null)
            {
                return false;
            }

            var oldValues =
                SerializePatient(patientExists);

            patientRepository.Update(patient);

            await patientRepository.SaveChangesAsync();

            var userId =
                await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues =
                    SerializePatient(patient);

                await auditLogService.LogAsync(
                    "Update",
                    "Patient",
                    patient.PatientId,
                    userId.Value,
                    oldValues,
                    newValues,
                    GetIpAddress());
            }

            return true;
        }

        // =========================================================
        // Activate
        // =========================================================

        public async Task<bool> ActivateAsync(int id)
        {
            var patient =
                await patientRepository.GetByIdAsync(id);

            if (patient == null)
            {
                return false;
            }

            var oldValues =
                SerializePatient(patient);

            patient.IsActive = true;

            patientRepository.Update(patient);

            await patientRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Activate",
                patient,
                oldValues);

            return true;
        }

        // =========================================================
        // Filtering
        // =========================================================

        public async Task<(
            List<Patient> Patients,
            int TotalCount)> FilterAsync(
                string? search,
                Gender? gender,
                string? bloodType,
                bool? isActive,
                int page,
                int pageSize)
        {
            return await patientRepository.FilterAsync(
                search,
                gender,
                bloodType,
                isActive,
                page,
                pageSize);
        }

        // =========================================================
        // Audit Log
        // =========================================================

        private string SerializePatient(
            Patient patient)
        {
            return JsonSerializer.Serialize(new
            {
                patient.PatientId,
                patient.PhoneNumber,
                patient.IsActive,
                patient.CreatedAt
            });
        }

        private async Task WriteAuditLogAsync(
            string action,
            Patient patient,
            string oldValues)
        {
            var userId =
                await GetCurrentUserIdAsync();

            if (!userId.HasValue)
            {
                return;
            }

            var newValues =
                SerializePatient(patient);

            await auditLogService.LogAsync(
                action,
                "Patient",
                patient.PatientId,
                userId.Value,
                oldValues,
                newValues,
                GetIpAddress());
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var userIdClaim =
                httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue(
                        ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return null;
            }

            var user =
                await userManager.FindByIdAsync(
                    userIdClaim);

            return user?.Id;
        }

        private string? GetIpAddress()
        {
            return httpContextAccessor.HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();
        }
    }
}
