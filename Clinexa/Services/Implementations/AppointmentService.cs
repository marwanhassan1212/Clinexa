using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;

namespace Clinexa.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository appointmentRepository;
        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;
        private readonly IDateTimeService dateTimeService;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IDateTimeService dateTimeService)
        {
            this.appointmentRepository = appointmentRepository;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.dateTimeService = dateTimeService;
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task<bool> CreateAsync(
            Appointment appointment)
        {
            // -----------------------------------------------------
            // Doctor can only create an appointment for himself.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return false;

                if (appointment.DoctorId !=
                    currentDoctorId.Value)
                {
                    return false;
                }
            }

            var now =
                dateTimeService.Now;

            var appointmentDate =
                appointment.AppointmentDate.Date;

            // -----------------------------------------------------
            // Appointment cannot be created in the past.
            // -----------------------------------------------------

            if (appointmentDate < now.Date)
                return false;

            // -----------------------------------------------------
            // Appointment cannot start in the past.
            // -----------------------------------------------------

            if (appointmentDate == now.Date &&
                appointment.StartTime <= now.TimeOfDay)
            {
                return false;
            }

            // -----------------------------------------------------
            // Start time must be before end time.
            // -----------------------------------------------------

            if (appointment.StartTime >=
                appointment.EndTime)
            {
                return false;
            }

            // -----------------------------------------------------
            // Doctor must exist.
            // -----------------------------------------------------

            bool doctorExists =
                await appointmentRepository
                    .DoctorExistsAsync(
                        appointment.DoctorId);

            if (!doctorExists)
                return false;

            // -----------------------------------------------------
            // Patient must exist.
            // -----------------------------------------------------

            bool patientExists =
                await appointmentRepository
                    .PatientExistsAsync(
                        appointment.PatientId);

            if (!patientExists)
                return false;

            // -----------------------------------------------------
            // Doctor must be working at this time.
            // -----------------------------------------------------

            bool isAvailable =
                await appointmentRepository
                    .IsDoctorAvailableAsync(
                        appointment.DoctorId,
                        appointment.AppointmentDate.DayOfWeek,
                        appointment.StartTime,
                        appointment.EndTime);

            if (!isAvailable)
                return false;

            // -----------------------------------------------------
            // Prevent overlapping appointments.
            // -----------------------------------------------------

            bool hasConflict =
                await appointmentRepository
                    .HasConflictAsync(
                        appointment.DoctorId,
                        appointment.AppointmentDate,
                        appointment.StartTime,
                        appointment.EndTime);

            if (hasConflict)
                return false;

            // -----------------------------------------------------
            // Default status.
            // -----------------------------------------------------

            appointment.AppointmentStatus =
                AppointmentStatus.Scheduled;

            appointment.CreatedAt =
                now;

            await appointmentRepository
                .AddAsync(appointment);

            await appointmentRepository
                .SaveChangesAsync();

            // =====================================================
            // AUDIT LOG
            // =====================================================

            var userId =
                await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues =
                    JsonSerializer.Serialize(new
                    {
                        appointment.AppointmentId,
                        appointment.DoctorId,
                        appointment.PatientId,
                        appointment.AppointmentDate,
                        appointment.StartTime,
                        appointment.EndTime,
                        appointment.AppointmentStatus,
                        appointment.CreatedAt
                    });

                await auditLogService.LogAsync(
                    "Create",
                    "Appointment",
                    appointment.AppointmentId,
                    userId.Value,
                    newValues: newValues,
                    ipAddress: GetIpAddress());
            }

            return true;
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public async Task<bool> UpdateAsync(
            Appointment appointment)
        {
            var existingAppointment =
                await appointmentRepository
                    .GetByIdAsync(
                        appointment.AppointmentId);

            if (existingAppointment == null)
                return false;

            // -----------------------------------------------------
            // Data-Level Authorization
            // -----------------------------------------------------

            if (!await CanAccessAppointmentAsync(
                    existingAppointment))
            {
                return false;
            }

            // -----------------------------------------------------
            // Doctor cannot move appointment to another doctor.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                if (appointment.DoctorId !=
                    existingAppointment.DoctorId)
                {
                    return false;
                }
            }

            // -----------------------------------------------------
            // Only Scheduled / Confirmed appointments
            // can be edited.
            // -----------------------------------------------------

            if (existingAppointment.AppointmentStatus !=
                    AppointmentStatus.Scheduled &&
                existingAppointment.AppointmentStatus !=
                    AppointmentStatus.Confirmed)
            {
                return false;
            }

            var now =
                dateTimeService.Now;

            var appointmentDate =
                appointment.AppointmentDate.Date;

            // -----------------------------------------------------
            // Appointment cannot be moved to the past.
            // -----------------------------------------------------

            if (appointmentDate < now.Date)
                return false;

            if (appointmentDate == now.Date &&
                appointment.StartTime <= now.TimeOfDay)
            {
                return false;
            }

            // -----------------------------------------------------
            // Start must be before End.
            // -----------------------------------------------------

            if (appointment.StartTime >=
                appointment.EndTime)
            {
                return false;
            }

            // -----------------------------------------------------
            // Doctor must exist.
            // -----------------------------------------------------

            bool doctorExists =
                await appointmentRepository
                    .DoctorExistsAsync(
                        appointment.DoctorId);

            if (!doctorExists)
                return false;

            // -----------------------------------------------------
            // Patient must exist.
            // -----------------------------------------------------

            bool patientExists =
                await appointmentRepository
                    .PatientExistsAsync(
                        appointment.PatientId);

            if (!patientExists)
                return false;

            // -----------------------------------------------------
            // Doctor must be available.
            // -----------------------------------------------------

            bool isAvailable =
                await appointmentRepository
                    .IsDoctorAvailableAsync(
                        appointment.DoctorId,
                        appointment.AppointmentDate.DayOfWeek,
                        appointment.StartTime,
                        appointment.EndTime);

            if (!isAvailable)
                return false;

            // -----------------------------------------------------
            // Prevent conflict.
            // -----------------------------------------------------

            bool hasConflict =
                await appointmentRepository
                    .HasConflictAsync(
                        appointment.DoctorId,
                        appointment.AppointmentDate,
                        appointment.StartTime,
                        appointment.EndTime,
                        appointment.AppointmentId);

            if (hasConflict)
                return false;

            // =====================================================
            // OLD VALUES
            // =====================================================

            var oldValues =
                JsonSerializer.Serialize(new
                {
                    existingAppointment.AppointmentId,
                    existingAppointment.DoctorId,
                    existingAppointment.PatientId,
                    existingAppointment.AppointmentDate,
                    existingAppointment.StartTime,
                    existingAppointment.EndTime,
                    existingAppointment.AppointmentStatus,
                    existingAppointment.ConfirmedAt,
                    existingAppointment.CheckedInAt,
                    existingAppointment.ConsultationStartedAt,
                    existingAppointment.CompletedAt,
                    existingAppointment.CancelledAt,
                    existingAppointment.CancellationReason,
                    existingAppointment.CreatedAt
                });

            // =====================================================
            // PRESERVE WORKFLOW FIELDS
            // =====================================================

            appointment.AppointmentStatus =
                existingAppointment.AppointmentStatus;

            appointment.ConfirmedAt =
                existingAppointment.ConfirmedAt;

            appointment.CheckedInAt =
                existingAppointment.CheckedInAt;

            appointment.ConsultationStartedAt =
                existingAppointment.ConsultationStartedAt;

            appointment.CompletedAt =
                existingAppointment.CompletedAt;

            appointment.CancelledAt =
                existingAppointment.CancelledAt;

            appointment.CancellationReason =
                existingAppointment.CancellationReason;

            appointment.CreatedAt =
                existingAppointment.CreatedAt;

            appointmentRepository.Update(
                appointment);

            await appointmentRepository
                .SaveChangesAsync();

            // =====================================================
            // NEW VALUES
            // =====================================================

            var newValues =
                JsonSerializer.Serialize(new
                {
                    appointment.AppointmentId,
                    appointment.DoctorId,
                    appointment.PatientId,
                    appointment.AppointmentDate,
                    appointment.StartTime,
                    appointment.EndTime,
                    appointment.AppointmentStatus,
                    appointment.ConfirmedAt,
                    appointment.CheckedInAt,
                    appointment.ConsultationStartedAt,
                    appointment.CompletedAt,
                    appointment.CancelledAt,
                    appointment.CancellationReason,
                    appointment.CreatedAt
                });

            // =====================================================
            // AUDIT LOG
            // =====================================================

            var userId =
                await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                await auditLogService.LogAsync(
                    "Update",
                    "Appointment",
                    appointment.AppointmentId,
                    userId.Value,
                    oldValues,
                    newValues,
                    GetIpAddress());
            }

            return true;
        }

        // =========================================================
        // CONFIRM
        // =========================================================

        public async Task<bool> ConfirmAsync(int id)
        {
            var appointment =
                await appointmentRepository
                    .GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (!await CanAccessAppointmentAsync(
                    appointment))
            {
                return false;
            }

            if (appointment.AppointmentStatus !=
                AppointmentStatus.Scheduled)
            {
                return false;
            }

            var oldValues =
                SerializeAppointment(
                    appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.Confirmed;

            appointment.ConfirmedAt =
                dateTimeService.Now;

            appointmentRepository.Update(
                appointment);

            await appointmentRepository
                .SaveChangesAsync();

            await WriteAuditLogAsync(
                "Confirm",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // CHECK IN
        // =========================================================

        public async Task<bool> CheckInAsync(int id)
        {
            var appointment =
                await appointmentRepository
                    .GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (!await CanAccessAppointmentAsync(
                    appointment))
            {
                return false;
            }

            if (appointment.AppointmentStatus !=
                AppointmentStatus.Confirmed)
            {
                return false;
            }

            var oldValues =
                SerializeAppointment(
                    appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.CheckedIn;

            appointment.CheckedInAt =
                dateTimeService.Now;

            appointmentRepository.Update(
                appointment);

            await appointmentRepository
                .SaveChangesAsync();

            await WriteAuditLogAsync(
                "CheckIn",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // START CONSULTATION
        // =========================================================

        public async Task<bool> StartConsultationAsync(
            int id)
        {
            var appointment =
                await appointmentRepository
                    .GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (!await CanAccessAppointmentAsync(
                    appointment))
            {
                return false;
            }

            if (appointment.AppointmentStatus !=
                AppointmentStatus.CheckedIn)
            {
                return false;
            }

            var oldValues =
                SerializeAppointment(
                    appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.InConsultation;

            appointment.ConsultationStartedAt =
                dateTimeService.Now;

            appointmentRepository.Update(
                appointment);

            await appointmentRepository
                .SaveChangesAsync();

            await WriteAuditLogAsync(
                "StartConsultation",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // COMPLETE
        // =========================================================

        public async Task<bool> CompleteAsync(int id)
        {
            var appointment =
                await appointmentRepository
                    .GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (!await CanAccessAppointmentAsync(
                    appointment))
            {
                return false;
            }

            if (appointment.AppointmentStatus !=
                AppointmentStatus.InConsultation)
            {
                return false;
            }

            var oldValues =
                SerializeAppointment(
                    appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.Completed;

            appointment.CompletedAt =
                dateTimeService.Now;

            appointmentRepository.Update(
                appointment);

            await appointmentRepository
                .SaveChangesAsync();

            await WriteAuditLogAsync(
                "Complete",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // CANCEL
        // =========================================================

        public async Task<bool> CancelAsync(
            int id,
            string? cancellationReason)
        {
            var appointment =
                await appointmentRepository
                    .GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (!await CanAccessAppointmentAsync(
                    appointment))
            {
                return false;
            }

            if (appointment.AppointmentStatus !=
                    AppointmentStatus.Scheduled &&
                appointment.AppointmentStatus !=
                    AppointmentStatus.Confirmed &&
                appointment.AppointmentStatus !=
                    AppointmentStatus.CheckedIn)
            {
                return false;
            }

            var oldValues =
                SerializeAppointment(
                    appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.Cancelled;

            appointment.CancelledAt =
                dateTimeService.Now;

            appointment.CancellationReason =
                cancellationReason?.Trim();

            appointmentRepository.Update(
                appointment);

            await appointmentRepository
                .SaveChangesAsync();

            await WriteAuditLogAsync(
                "Cancel",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // NO SHOW
        // =========================================================

        public async Task<bool> MarkAsNoShowAsync(
            int id)
        {
            var appointment =
                await appointmentRepository
                    .GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (!await CanAccessAppointmentAsync(
                    appointment))
            {
                return false;
            }

            if (appointment.AppointmentStatus !=
                    AppointmentStatus.Scheduled &&
                appointment.AppointmentStatus !=
                    AppointmentStatus.Confirmed)
            {
                return false;
            }

            var oldValues =
                SerializeAppointment(
                    appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.NoShow;

            appointmentRepository.Update(
                appointment);

            await appointmentRepository
                .SaveChangesAsync();

            await WriteAuditLogAsync(
                "NoShow",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<List<Appointment>>
            GetAllAsync()
        {
            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return new List<Appointment>();

                return await appointmentRepository
                    .GetByDoctorIdAsync(
                        currentDoctorId.Value);
            }

            return await appointmentRepository
                .GetAllAsync();
        }

        // =========================================================
        // GET BY DOCTOR
        // =========================================================

        public async Task<List<Appointment>>
            GetByDoctorIdAsync(
                int doctorId)
        {
            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return new List<Appointment>();

                if (doctorId != currentDoctorId.Value)
                    return new List<Appointment>();
            }

            return await appointmentRepository
                .GetByDoctorIdAsync(
                    doctorId);
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<Appointment?>
            GetByIdAsync(
                int id)
        {
            var appointment =
                await appointmentRepository
                    .GetByIdAsync(id);

            if (appointment == null)
                return null;

            if (!await CanAccessAppointmentAsync(
                    appointment))
            {
                return null;
            }

            return appointment;
        }

        // =========================================================
        // GET BY PATIENT
        // =========================================================

        public async Task<List<Appointment>>
            GetByPatientIdAsync(
                int patientId)
        {
            var appointments =
                await appointmentRepository
                    .GetByPatientIdAsync(
                        patientId);

            if (!IsCurrentUserDoctor())
                return appointments;

            var currentDoctorId =
                await GetCurrentDoctorIdAsync();

            if (!currentDoctorId.HasValue)
                return new List<Appointment>();

            return appointments
                .Where(x =>
                    x.DoctorId ==
                    currentDoctorId.Value)
                .ToList();
        }

        // =========================================================
        // FILTER
        // =========================================================

        public async Task<(
            List<Appointment> Appointments,
            int TotalCount)> FilterAsync(
            string? search,
            int? doctorId,
            int? patientId,
            DateTime? appointmentDate,
            AppointmentStatus? status,
            int page,
            int pageSize)
        {
            // -----------------------------------------------------
            // Doctor is always scoped to himself.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                {
                    return (
                        new List<Appointment>(),
                        0);
                }

                /*
                 * Ignore DoctorId supplied by the client.
                 *
                 * The logged-in Doctor is always the scope.
                 */

                doctorId =
                    currentDoctorId.Value;
            }

            return await appointmentRepository
                .FilterAsync(
                    search,
                    doctorId,
                    patientId,
                    appointmentDate,
                    status,
                    page,
                    pageSize);
        }

        // =========================================================
        // AVAILABLE SLOTS
        // =========================================================

        public async Task<List<TimeSpan>>
            GetAvailableSlotsAsync(
                int doctorId,
                DateTime appointmentDate)
        {
            // -----------------------------------------------------
            // Doctor can only request his own slots.
            // -----------------------------------------------------

            if (IsCurrentUserDoctor())
            {
                var currentDoctorId =
                    await GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                    return new List<TimeSpan>();

                if (doctorId !=
                    currentDoctorId.Value)
                {
                    return new List<TimeSpan>();
                }
            }

            var dayOfWeek =
                appointmentDate.DayOfWeek;

            var schedules =
                await appointmentRepository
                    .GetDoctorSchedulesAsync(
                        doctorId,
                        dayOfWeek);

            if (!schedules.Any())
                return new List<TimeSpan>();

            var appointments =
                await appointmentRepository
                    .GetDoctorAppointmentsAsync(
                        doctorId,
                        appointmentDate);

            const int slotDurationMinutes = 30;

            var slots =
                new List<TimeSpan>();

            foreach (var schedule in schedules)
            {
                var currentTime =
                    schedule.StartTime;

                while (currentTime.Add(
                           TimeSpan.FromMinutes(
                               slotDurationMinutes))
                       <= schedule.EndTime)
                {
                    var slotEndTime =
                        currentTime.Add(
                            TimeSpan.FromMinutes(
                                slotDurationMinutes));

                    bool isBooked =
                        appointments.Any(x =>
                            x.AppointmentStatus !=
                                AppointmentStatus.Completed &&
                            x.AppointmentStatus !=
                                AppointmentStatus.Cancelled &&
                            x.AppointmentStatus !=
                                AppointmentStatus.NoShow &&
                            x.StartTime < slotEndTime &&
                            x.EndTime > currentTime);

                    if (!isBooked)
                    {
                        slots.Add(currentTime);
                    }

                    currentTime =
                        slotEndTime;
                }
            }

            return slots
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        // =========================================================
        // DATA-LEVEL AUTHORIZATION
        // =========================================================

        private bool IsCurrentUserDoctor()
        {
            return httpContextAccessor
                .HttpContext?
                .User
                .IsInRole("Doctor") == true;
        }

        private async Task<bool>
            CanAccessAppointmentAsync(
                Appointment appointment)
        {
            /*
             * Admin / Receptionist:
             * Controller policy already gives them access.
             *
             * Doctor:
             * must own the appointment.
             */

            if (!IsCurrentUserDoctor())
                return true;

            var currentDoctorId =
                await GetCurrentDoctorIdAsync();

            if (!currentDoctorId.HasValue)
                return false;

            return appointment.DoctorId ==
                   currentDoctorId.Value;
        }

        // =========================================================
        // CURRENT DOCTOR
        // =========================================================

        public async Task<int?>
            GetCurrentDoctorIdAsync()
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
                    out var userId))
            {
                return null;
            }

            var user =
                await userManager
                    .FindByIdAsync(
                        userId.ToString());

            if (user == null)
                return null;

            var doctor =
                await appointmentRepository
                    .GetDoctorByUserIdAsync(
                        user.Id);

            return doctor?.DoctorId;
        }

        // =========================================================
        // AUDIT LOG
        // =========================================================

        private string SerializeAppointment(
            Appointment appointment)
        {
            return JsonSerializer.Serialize(
                new
                {
                    appointment.AppointmentId,
                    appointment.DoctorId,
                    appointment.PatientId,
                    appointment.AppointmentDate,
                    appointment.StartTime,
                    appointment.EndTime,
                    appointment.AppointmentStatus,
                    appointment.ConfirmedAt,
                    appointment.CheckedInAt,
                    appointment.ConsultationStartedAt,
                    appointment.CompletedAt,
                    appointment.CancelledAt,
                    appointment.CancellationReason,
                    appointment.CreatedAt
                });
        }

        private async Task WriteAuditLogAsync(
            string action,
            Appointment appointment,
            string oldValues)
        {
            var userId =
                await GetCurrentUserIdAsync();

            if (!userId.HasValue)
                return;

            var newValues =
                SerializeAppointment(
                    appointment);

            await auditLogService.LogAsync(
                action,
                "Appointment",
                appointment.AppointmentId,
                userId.Value,
                oldValues,
                newValues,
                GetIpAddress());
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
                    out var userId))
            {
                return null;
            }

            var user =
                await userManager
                    .FindByIdAsync(
                        userId.ToString());

            return user?.Id;
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

        // =========================================================
        // TIME TEST
        // =========================================================

        public string TestTime()
        {
            return
                $"UTC: {DateTime.UtcNow}\n" +
                $"System Local: {DateTime.Now}\n" +
                $"Egypt Service: {dateTimeService.Now}";
        }
    }
}