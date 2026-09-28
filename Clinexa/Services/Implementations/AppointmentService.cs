using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Channels;

namespace Clinexa.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository appointmentRepository;
        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            this.appointmentRepository = appointmentRepository;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }

        // =========================================================
        // Create
        // =========================================================

        public async Task<bool> CreateAsync(Appointment appointment)
        {
            if (appointment.AppointmentDate.Date < DateTime.Today)
                return false;

            if (appointment.StartTime >= appointment.EndTime)
                return false;

            bool doctorExists =
                await appointmentRepository.DoctorExistsAsync(
                    appointment.DoctorId);

            if (!doctorExists)
                return false;

            bool patientExists =
                await appointmentRepository.PatientExistsAsync(
                    appointment.PatientId);

            if (!patientExists)
                return false;

            bool isAvailable =
                await appointmentRepository.IsDoctorAvailableAsync(
                    appointment.DoctorId,
                    appointment.AppointmentDate.DayOfWeek,
                    appointment.StartTime,
                    appointment.EndTime);

            if (!isAvailable)
                return false;

            bool hasConflict =
                await appointmentRepository.HasConflictAsync(
                    appointment.DoctorId,
                    appointment.AppointmentDate,
                    appointment.StartTime,
                    appointment.EndTime);

            if (hasConflict)
                return false;

            appointment.AppointmentStatus =
                AppointmentStatus.Scheduled;

            appointment.CreatedAt =
                DateTime.UtcNow;

            await appointmentRepository.AddAsync(appointment);
            await appointmentRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
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
        // Update
        // =========================================================

        public async Task<bool> UpdateAsync(Appointment appointment)
        {
            var existingAppointment =
                await appointmentRepository.GetByIdAsync(
                    appointment.AppointmentId);

            if (existingAppointment == null)
                return false;

            // Appointment details can only be changed
            // before the patient checks in.
            if (existingAppointment.AppointmentStatus != AppointmentStatus.Scheduled &&
                existingAppointment.AppointmentStatus != AppointmentStatus.Confirmed)
            {
                return false;
            }

            if (appointment.AppointmentDate.Date < DateTime.Today)
                return false;

            if (appointment.StartTime >= appointment.EndTime)
                return false;

            bool doctorExists =
                await appointmentRepository.DoctorExistsAsync(
                    appointment.DoctorId);

            if (!doctorExists)
                return false;

            bool patientExists =
                await appointmentRepository.PatientExistsAsync(
                    appointment.PatientId);

            if (!patientExists)
                return false;

            bool isAvailable =
                await appointmentRepository.IsDoctorAvailableAsync(
                    appointment.DoctorId,
                    appointment.AppointmentDate.DayOfWeek,
                    appointment.StartTime,
                    appointment.EndTime);

            if (!isAvailable)
                return false;

            bool hasConflict =
                await appointmentRepository.HasConflictAsync(
                    appointment.DoctorId,
                    appointment.AppointmentDate,
                    appointment.StartTime,
                    appointment.EndTime,
                    appointment.AppointmentId);

            if (hasConflict)
                return false;

            var oldValues = JsonSerializer.Serialize(new
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

            // Preserve the existing workflow state.
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

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
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
        // Confirm
        // =========================================================

        public async Task<bool> ConfirmAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.Scheduled)
                return false;

            var oldValues = SerializeAppointment(appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.Confirmed;

            appointment.ConfirmedAt =
                DateTime.UtcNow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Confirm",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // Check In
        // =========================================================

        public async Task<bool> CheckInAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.Confirmed)
                return false;

            var oldValues = SerializeAppointment(appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.CheckedIn;

            appointment.CheckedInAt =
                DateTime.UtcNow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "CheckIn",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // Start Consultation
        // =========================================================

        public async Task<bool> StartConsultationAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.CheckedIn)
                return false;

            var oldValues = SerializeAppointment(appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.InConsultation;

            appointment.ConsultationStartedAt =
                DateTime.UtcNow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "StartConsultation",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // Complete
        // =========================================================

        public async Task<bool> CompleteAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.InConsultation)
                return false;

            var oldValues = SerializeAppointment(appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.Completed;

            appointment.CompletedAt =
                DateTime.UtcNow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Complete",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // Cancel
        // =========================================================

        public async Task<bool> CancelAsync(
            int id,
            string? cancellationReason)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.Scheduled &&
                appointment.AppointmentStatus != AppointmentStatus.Confirmed &&
                appointment.AppointmentStatus != AppointmentStatus.CheckedIn)
            {
                return false;
            }

            var oldValues = SerializeAppointment(appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.Cancelled;

            appointment.CancelledAt =
                DateTime.UtcNow;

            appointment.CancellationReason =
                cancellationReason?.Trim();

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Cancel",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // No Show
        // =========================================================

        public async Task<bool> MarkAsNoShowAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.Scheduled &&
                appointment.AppointmentStatus != AppointmentStatus.Confirmed)
            {
                return false;
            }

            var oldValues = SerializeAppointment(appointment);

            appointment.AppointmentStatus =
                AppointmentStatus.NoShow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "NoShow",
                appointment,
                oldValues);

            return true;
        }

        // =========================================================
        // Get
        // =========================================================

        public async Task<List<Appointment>> GetAllAsync()
        {
            return await appointmentRepository.GetAllAsync();
        }

        public async Task<List<Appointment>> GetByDoctorIdAsync(
            int doctorId)
        {
            return await appointmentRepository
                .GetByDoctorIdAsync(doctorId);
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await appointmentRepository
                .GetByIdAsync(id);
        }

        public async Task<List<Appointment>> GetByPatientIdAsync(
            int patientId)
        {
            return await appointmentRepository
                .GetByPatientIdAsync(patientId);
        }

        // =========================================================
        // Filtering
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
            return await appointmentRepository.FilterAsync(
                search,
                doctorId,
                patientId,
                appointmentDate,
                status,
                page,
                pageSize);
        }

        // =========================================================
        // Available Slots
        // =========================================================

        public async Task<List<TimeSpan>> GetAvailableSlotsAsync(
            int doctorId,
            DateTime appointmentDate)
        {
            var dayOfWeek =
                appointmentDate.DayOfWeek;

            var schedules =
                await appointmentRepository
                    .GetDoctorSchedulesAsync(
                        doctorId,
                        dayOfWeek);

            if (!schedules.Any())
            {
                return new List<TimeSpan>();
            }

            var appointments =
                await appointmentRepository
                    .GetDoctorAppointmentsAsync(
                        doctorId,
                        appointmentDate);

            const int slotDurationMinutes = 30;

            var slots = new List<TimeSpan>();

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
                            x.StartTime < slotEndTime &&
                            x.EndTime > currentTime);

                    if (!isBooked)
                    {
                        slots.Add(currentTime);
                    }

                    currentTime = slotEndTime;
                }
            }

            return slots
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        // =========================================================
        // Audit Log
        // =========================================================

        private string SerializeAppointment(
            Appointment appointment)
        {
            return JsonSerializer.Serialize(new
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
            var userId = await GetCurrentUserIdAsync();

            if (!userId.HasValue)
                return;

            var newValues =
                SerializeAppointment(appointment);

            await auditLogService.LogAsync(
                action,
                "Appointment",
                appointment.AppointmentId,
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
                return null;

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