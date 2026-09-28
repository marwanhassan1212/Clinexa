using System.Text.Json;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Clinexa.Services.Implementations
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository medicalRecordRepository;
        private readonly UserManager<User> userManager;
        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public MedicalRecordService(
            IMedicalRecordRepository medicalRecordRepository,
            UserManager<User> userManager,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.medicalRecordRepository = medicalRecordRepository;
            this.userManager = userManager;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
        }


        // =========================================================
        // CREATE
        // =========================================================

        public async Task<bool> CreateAsync(
            MedicalRecord medicalRecord)
        {
            bool appointmentExists =
                await medicalRecordRepository
                    .AppointmentExistsAsync(
                        medicalRecord.AppointmentId);

            if (!appointmentExists)
            {
                return false;
            }


            bool appointmentCompleted =
                await medicalRecordRepository
                    .IsAppointmentCompletedAsync(
                        medicalRecord.AppointmentId);

            if (!appointmentCompleted)
            {
                return false;
            }


            bool doctorExists =
                await medicalRecordRepository
                    .DoctorExistsAsync(
                        medicalRecord.DoctorId);

            if (!doctorExists)
            {
                return false;
            }


            bool doctorAssigned =
                await medicalRecordRepository
                    .IsDoctorAssignedToAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.DoctorId);

            if (!doctorAssigned)
            {
                return false;
            }


            bool recordExists =
                await medicalRecordRepository
                    .ExistsForAppointmentAsync(
                        medicalRecord.AppointmentId);

            if (recordExists)
            {
                return false;
            }


            bool patientAssigned =
                await medicalRecordRepository
                    .IsPatientAssignedToAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.PatientId);

            if (!patientAssigned)
            {
                return false;
            }


            await medicalRecordRepository
                .AddAsync(medicalRecord);


            await medicalRecordRepository
                .SaveChangesAsync();


            // =====================================================
            // AUDIT LOG
            // =====================================================

            var currentUserId =
                await GetCurrentUserIdAsync();

            if (currentUserId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(
                    new
                    {
                        medicalRecord.MedicalRecordId,
                        medicalRecord.AppointmentId,
                        medicalRecord.PatientId,
                        medicalRecord.DoctorId
                    });


                await auditLogService.LogAsync(
                    action: "Create",
                    entityName: "MedicalRecord",
                    entityId: medicalRecord.MedicalRecordId,
                    userId: currentUserId.Value,
                    oldValues: null,
                    newValues: newValues,
                    ipAddress: GetIpAddress());
            }


            return true;
        }


        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<List<MedicalRecord>> GetAllAsync()
        {
            return await medicalRecordRepository
                .GetAllAsync();
        }


        // =========================================================
        // GET BY APPOINTMENT
        // =========================================================

        public async Task<MedicalRecord?> GetByAppointmentIdAsync(
            int appointmentId)
        {
            return await medicalRecordRepository
                .GetByAppointmentIdAsync(
                    appointmentId);
        }


        // =========================================================
        // GET BY DOCTOR
        // =========================================================

        public async Task<List<MedicalRecord>> GetByDoctorIdAsync(
            int doctorId)
        {
            return await medicalRecordRepository
                .GetByDoctorIdAsync(
                    doctorId);
        }


        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<MedicalRecord?> GetByIdAsync(
            int id)
        {
            return await medicalRecordRepository
                .GetByIdAsync(id);
        }


        // =========================================================
        // GET BY PATIENT
        // =========================================================

        public async Task<List<MedicalRecord>> GetByPatientIdAsync(
            int patientId)
        {
            return await medicalRecordRepository
                .GetByPatientIdAsync(
                    patientId);
        }


        // =========================================================
        // UPDATE
        // =========================================================

        public async Task<bool> UpdateAsync(
            MedicalRecord medicalRecord)
        {
            // -----------------------------------------------------
            // Get existing record BEFORE modifying it.
            // This is required for OldValues.
            // -----------------------------------------------------

            var medicalRecordExists =
                await medicalRecordRepository
                    .GetByIdAsync(
                        medicalRecord.MedicalRecordId);

            if (medicalRecordExists == null)
            {
                return false;
            }


            bool appointmentExists =
                await medicalRecordRepository
                    .AppointmentExistsAsync(
                        medicalRecord.AppointmentId);

            if (!appointmentExists)
            {
                return false;
            }


            bool doctorExists =
                await medicalRecordRepository
                    .DoctorExistsAsync(
                        medicalRecord.DoctorId);

            if (!doctorExists)
            {
                return false;
            }


            bool doctorAssigned =
                await medicalRecordRepository
                    .IsDoctorAssignedToAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.DoctorId);

            if (!doctorAssigned)
            {
                return false;
            }


            bool patientAssigned =
                await medicalRecordRepository
                    .IsPatientAssignedToAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.PatientId);

            if (!patientAssigned)
            {
                return false;
            }


            bool duplicate =
                await medicalRecordRepository
                    .ExistsForAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.MedicalRecordId);

            if (duplicate)
            {
                return false;
            }


            // =====================================================
            // OLD VALUES
            // =====================================================

            var oldValues = JsonSerializer.Serialize(
                new
                {
                    medicalRecordExists.MedicalRecordId,
                    medicalRecordExists.AppointmentId,
                    medicalRecordExists.PatientId,
                    medicalRecordExists.DoctorId
                });


            // =====================================================
            // UPDATE DATABASE
            // =====================================================

            medicalRecordRepository.Update(
                medicalRecord);


            await medicalRecordRepository
                .SaveChangesAsync();


            // =====================================================
            // NEW VALUES
            // =====================================================

            var newValues = JsonSerializer.Serialize(
                new
                {
                    medicalRecord.MedicalRecordId,
                    medicalRecord.AppointmentId,
                    medicalRecord.PatientId,
                    medicalRecord.DoctorId
                });


            // =====================================================
            // AUDIT LOG
            // =====================================================

            var currentUserId =
                await GetCurrentUserIdAsync();

            if (currentUserId.HasValue)
            {
                await auditLogService.LogAsync(
                    action: "Update",
                    entityName: "MedicalRecord",
                    entityId: medicalRecord.MedicalRecordId,
                    userId: currentUserId.Value,
                    oldValues: oldValues,
                    newValues: newValues,
                    ipAddress: GetIpAddress());
            }


            return true;
        }


        // =========================================================
        // AVAILABLE APPOINTMENTS
        // =========================================================

        public async Task<List<Appointment>>
            GetAvailableAppointmentsAsync()
        {
            return await medicalRecordRepository
                .GetAvailableAppointmentsAsync();
        }


        // =========================================================
        // APPOINTMENT BY ID
        // =========================================================

        public async Task<Appointment?>
            GetAppointmentByIdAsync(
                int appointmentId)
        {
            return await medicalRecordRepository
                .GetAppointmentByIdAsync(
                    appointmentId);
        }


        // =========================================================
        // AUTHORIZATION
        // =========================================================

        public async Task<bool> CanAccessAsync(
            int medicalRecordId,
            int currentUserId)
        {
            var record =
                await medicalRecordRepository
                    .GetByIdAsync(
                        medicalRecordId);

            if (record == null)
            {
                return false;
            }


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


            // Doctor can access only his own records.
            return record.Doctor.UserId ==
                   currentUserId;
        }


        // =========================================================
        // GET DOCTOR ID BY USER ID
        // =========================================================

        public async Task<int?>
            GetDoctorIdByUserIdAsync(
                int userId)
        {
            return await medicalRecordRepository
                .GetDoctorIdByUserIdAsync(
                    userId);
        }


        // =========================================================
        // CURRENT USER
        // =========================================================

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var userId =
                httpContextAccessor
                    .HttpContext?
                    .User?
                    .FindFirst(
                        System.Security.Claims.ClaimTypes.NameIdentifier)?
                    .Value;

            if (!int.TryParse(
                    userId,
                    out var currentUserId))
            {
                return null;
            }

            var user =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (user == null)
            {
                return null;
            }

            return currentUserId;
        }


        // =========================================================
        // IP ADDRESS
        // =========================================================

        private string? GetIpAddress()
        {
            return httpContextAccessor
                .HttpContext?
                .Connection?
                .RemoteIpAddress?
                .ToString();
        }
    }
}