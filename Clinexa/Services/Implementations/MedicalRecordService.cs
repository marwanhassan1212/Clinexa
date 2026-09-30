using System.Security.Claims;
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
        private readonly IDateTimeService dateTimeService;

        public MedicalRecordService(
            IMedicalRecordRepository medicalRecordRepository,
            UserManager<User> userManager,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeService dateTimeService)
        {
            this.medicalRecordRepository = medicalRecordRepository;
            this.userManager = userManager;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.dateTimeService = dateTimeService;
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task<bool> CreateAsync(MedicalRecord medicalRecord)
        {
            // -----------------------------------------------------
            // If current user is Doctor,
            // he can only create a MedicalRecord for his own
            // appointment.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                var currentDoctorId = await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return false;

                if (medicalRecord.DoctorId != currentDoctorId.Value)
                    return false;
            }

            // -----------------------------------------------------
            // Appointment must exist.
            // -----------------------------------------------------

            bool appointmentExists =
                await medicalRecordRepository
                    .AppointmentExistsAsync(
                        medicalRecord.AppointmentId);

            if (!appointmentExists)
                return false;

            // -----------------------------------------------------
            // Appointment must be completed.
            // -----------------------------------------------------

            bool appointmentCompleted =
                await medicalRecordRepository
                    .IsAppointmentCompletedAsync(
                        medicalRecord.AppointmentId);

            if (!appointmentCompleted)
                return false;

            // -----------------------------------------------------
            // Doctor must exist.
            // -----------------------------------------------------

            bool doctorExists =
                await medicalRecordRepository
                    .DoctorExistsAsync(
                        medicalRecord.DoctorId);

            if (!doctorExists)
                return false;

            // -----------------------------------------------------
            // Doctor must be assigned to this appointment.
            // -----------------------------------------------------

            bool doctorAssigned =
                await medicalRecordRepository
                    .IsDoctorAssignedToAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.DoctorId);

            if (!doctorAssigned)
                return false;

            // -----------------------------------------------------
            // Only one Medical Record per Appointment.
            // -----------------------------------------------------

            bool recordExists =
                await medicalRecordRepository
                    .ExistsForAppointmentAsync(
                        medicalRecord.AppointmentId);

            if (recordExists)
                return false;

            // -----------------------------------------------------
            // Patient must belong to the appointment.
            // -----------------------------------------------------

            bool patientAssigned =
                await medicalRecordRepository
                    .IsPatientAssignedToAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.PatientId);

            if (!patientAssigned)
                return false;

            // -----------------------------------------------------
            // Create
            // -----------------------------------------------------

            medicalRecord.CreatedAt =
                dateTimeService.Now;

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
                var newValues =
                    SerializeMedicalRecord(medicalRecord);

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
            // -----------------------------------------------------
            // Doctor can only see his own Medical Records.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return new List<MedicalRecord>();

                return await medicalRecordRepository
                    .GetByDoctorIdAsync(
                        currentDoctorId.Value);
            }

            // -----------------------------------------------------
            // Admin / other authorized clinical users
            // can see all records.
            // -----------------------------------------------------

            return await medicalRecordRepository
                .GetAllAsync();
        }

        // =========================================================
        // GET BY APPOINTMENT
        // =========================================================

        public async Task<MedicalRecord?>
            GetByAppointmentIdAsync(int appointmentId)
        {
            var medicalRecord =
                await medicalRecordRepository
                    .GetByAppointmentIdAsync(
                        appointmentId);

            if (medicalRecord == null)
                return null;

            // -----------------------------------------------------
            // Doctor can only access his own record.
            // -----------------------------------------------------

            if (!await CanAccessMedicalRecordAsync(
                    medicalRecord))
            {
                return null;
            }

            return medicalRecord;
        }

        // =========================================================
        // GET BY DOCTOR
        // =========================================================

        public async Task<List<MedicalRecord>>
            GetByDoctorIdAsync(int doctorId)
        {
            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return new List<MedicalRecord>();

                if (doctorId != currentDoctorId.Value)
                    return new List<MedicalRecord>();
            }

            return await medicalRecordRepository
                .GetByDoctorIdAsync(
                    doctorId);
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<MedicalRecord?>
            GetByIdAsync(int id)
        {
            var medicalRecord =
                await medicalRecordRepository
                    .GetByIdAsync(id);

            if (medicalRecord == null)
                return null;

            // -----------------------------------------------------
            // Data-Level Authorization
            // -----------------------------------------------------

            if (!await CanAccessMedicalRecordAsync(
                    medicalRecord))
            {
                return null;
            }

            return medicalRecord;
        }

        // =========================================================
        // GET BY PATIENT
        // =========================================================

        public async Task<List<MedicalRecord>>
            GetByPatientIdAsync(int patientId)
        {
            // -----------------------------------------------------
            // Doctor:
            // only records of this patient that belong to
            // the current doctor.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return new List<MedicalRecord>();

                var doctorRecords =
                    await medicalRecordRepository
                        .GetByDoctorIdAsync(
                            currentDoctorId.Value);

                return doctorRecords
                    .Where(x => x.PatientId == patientId)
                    .ToList();
            }

            // -----------------------------------------------------
            // Admin / authorized users
            // -----------------------------------------------------

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
            // Get existing record before modifying.
            // -----------------------------------------------------

            var existingMedicalRecord =
                await medicalRecordRepository
                    .GetByIdAsync(
                        medicalRecord.MedicalRecordId);

            if (existingMedicalRecord == null)
                return false;

            // -----------------------------------------------------
            // DATA-LEVEL AUTHORIZATION
            // -----------------------------------------------------

            if (!await CanAccessMedicalRecordAsync(
                    existingMedicalRecord))
            {
                return false;
            }

            // -----------------------------------------------------
            // Appointment must exist.
            // -----------------------------------------------------

            bool appointmentExists =
                await medicalRecordRepository
                    .AppointmentExistsAsync(
                        existingMedicalRecord.AppointmentId);

            if (!appointmentExists)
                return false;

            // -----------------------------------------------------
            // Appointment must be completed.
            // -----------------------------------------------------

            bool appointmentCompleted =
                await medicalRecordRepository
                    .IsAppointmentCompletedAsync(
                        existingMedicalRecord.AppointmentId);

            if (!appointmentCompleted)
                return false;

            // -----------------------------------------------------
            // Preserve relationship data.
            //
            // The user should NOT be able to change:
            // AppointmentId
            // PatientId
            // DoctorId
            // -----------------------------------------------------

            medicalRecord.AppointmentId =
                existingMedicalRecord.AppointmentId;

            medicalRecord.PatientId =
                existingMedicalRecord.PatientId;

            medicalRecord.DoctorId =
                existingMedicalRecord.DoctorId;

            medicalRecord.CreatedAt =
                existingMedicalRecord.CreatedAt;

            medicalRecord.UpdatedAt =
                dateTimeService.Now;

            // =====================================================
            // OLD VALUES
            // =====================================================

            var oldValues =
                SerializeMedicalRecord(
                    existingMedicalRecord);

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

            var newValues =
                SerializeMedicalRecord(
                    medicalRecord);

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
            // -----------------------------------------------------
            // Doctor only gets his own completed appointments
            // that don't already have a Medical Record.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return new List<Appointment>();

                return await medicalRecordRepository
                    .GetAvailableAppointmentsByDoctorIdAsync(
                        currentDoctorId.Value);
            }

            // -----------------------------------------------------
            // Admin / authorized clinical users
            // -----------------------------------------------------

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
            var appointment =
                await medicalRecordRepository
                    .GetAppointmentByIdAsync(
                        appointmentId);

            if (appointment == null)
                return null;

            // -----------------------------------------------------
            // Doctor can only access his own appointment.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return null;

                if (appointment.DoctorId != currentDoctorId.Value)
                    return null;
            }

            return appointment;
        }

        // =========================================================
        // AUTHORIZATION
        // =========================================================

        public async Task<bool> CanAccessAsync(
            int medicalRecordId,
            int currentUserId)
        {
            var medicalRecord =
                await medicalRecordRepository
                    .GetByIdAsync(
                        medicalRecordId);

            if (medicalRecord == null)
                return false;

            var user =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (user == null)
                return false;

            // -----------------------------------------------------
            // Admin can access all records.
            // -----------------------------------------------------

            if (await userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                return true;
            }

            // -----------------------------------------------------
            // Doctor can access only his own records.
            // -----------------------------------------------------

            if (await userManager.IsInRoleAsync(
                    user,
                    "Doctor"))
            {
                var doctorId =
                    await medicalRecordRepository
                        .GetDoctorIdByUserIdAsync(
                            currentUserId);

                if (!doctorId.HasValue)
                    return false;

                return medicalRecord.DoctorId ==
                       doctorId.Value;
            }

            return false;
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
        // CURRENT DOCTOR
        // =========================================================

        private async Task<int?>
            GetCurrentDoctorIdAsync()
        {
            var currentUserId =
                await GetCurrentUserIdAsync();

            if (!currentUserId.HasValue)
                return null;

            return await medicalRecordRepository
                .GetDoctorIdByUserIdAsync(
                    currentUserId.Value);
        }

        // =========================================================
        // CURRENT USER IS DOCTOR
        // =========================================================

        private bool IsCurrentUserDoctor()
        {
            return httpContextAccessor
                .HttpContext?
                .User
                .IsInRole("Doctor") == true;
        }

        // =========================================================
        // MEDICAL RECORD ACCESS
        // =========================================================

        private async Task<bool>
            CanAccessMedicalRecordAsync(
                MedicalRecord medicalRecord)
        {
            // -----------------------------------------------------
            // Admin / non-Doctor clinical users
            // -----------------------------------------------------

            if (!IsCurrentUserDoctor())
                return true;

            // -----------------------------------------------------
            // Doctor
            // -----------------------------------------------------

            var currentDoctorId =
                await GetCurrentDoctorIdAsync();

            if (!currentDoctorId.HasValue)
                return false;

            return medicalRecord.DoctorId ==
                   currentDoctorId.Value;
        }

        // =========================================================
        // CURRENT USER
        // =========================================================

        private async Task<int?>
            GetCurrentUserIdAsync()
        {
            var userIdClaim =
                httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirstValue(
                        ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (!int.TryParse(
                    userIdClaim,
                    out var currentUserId))
            {
                return null;
            }

            var user =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (user == null)
                return null;

            return currentUserId;
        }

        // =========================================================
        // SERIALIZE MEDICAL RECORD
        // =========================================================

        private string SerializeMedicalRecord(
            MedicalRecord medicalRecord)
        {
            return JsonSerializer.Serialize(
                new
                {
                    medicalRecord.MedicalRecordId,
                    medicalRecord.AppointmentId,
                    medicalRecord.PatientId,
                    medicalRecord.DoctorId,
                    medicalRecord.Symptoms,
                    medicalRecord.Diagnosis,
                    medicalRecord.Treatment,
                    medicalRecord.Notes,
                    medicalRecord.FollowUpDate,
                    medicalRecord.CreatedAt,
                    medicalRecord.UpdatedAt
                });
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