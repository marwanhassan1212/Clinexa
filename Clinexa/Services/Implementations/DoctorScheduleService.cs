using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;

namespace Clinexa.Services.Implementations
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IDoctorScheduleRepository doctorScheduleRepository;

        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;
        private readonly IDateTimeService dateTimeService;
        public DoctorScheduleService(
            IDoctorScheduleRepository doctorScheduleRepository,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IDateTimeService dateTimeService)
        {
            this.doctorScheduleRepository = doctorScheduleRepository;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.dateTimeService = dateTimeService;
        }

        public async Task<bool> CreateAsync(DoctorSchedule schedule)
        {
            bool doctorExists =
                await doctorScheduleRepository.DoctorExistsAsync(
                    schedule.DoctorId);

            if (!doctorExists)
                return false;

            if (schedule.StartTime >= schedule.EndTime)
                return false;

            bool hasDuplicateStartTime =
                await doctorScheduleRepository.HasDuplicateStartTimeAsync(
                    schedule.DoctorId,
                    schedule.DayOfWeek,
                    schedule.StartTime);

            if (hasDuplicateStartTime)
                return false;

            bool hasOverlap =
                await doctorScheduleRepository.HasOverlapAsync(
                    schedule.DoctorId,
                    schedule.DayOfWeek,
                    schedule.StartTime,
                    schedule.EndTime);

            if (hasOverlap)
                return false;

            schedule.IsAvailable = true;

            await doctorScheduleRepository.AddAsync(schedule);
            await doctorScheduleRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Create",
                schedule,
                null);

            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var schedule =
                await doctorScheduleRepository.GetByIdAsync(id);

            if (schedule == null)
                return false;

            var oldValues = SerializeSchedule(schedule);

            schedule.IsAvailable = false;

            doctorScheduleRepository.Update(schedule);

            await doctorScheduleRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Deactivate",
                schedule,
                oldValues);

            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var schedule =
                await doctorScheduleRepository.GetByIdAsync(id);

            if (schedule == null)
                return false;

            bool doctorExists =
                await doctorScheduleRepository.DoctorExistsAsync(
                    schedule.DoctorId);

            if (!doctorExists)
                return false;

            bool hasDuplicateStartTime =
                await doctorScheduleRepository.HasDuplicateStartTimeAsync(
                    schedule.DoctorId,
                    schedule.DayOfWeek,
                    schedule.StartTime,
                    schedule.DoctorScheduleId);

            if (hasDuplicateStartTime)
                return false;

            bool hasOverlap =
                await doctorScheduleRepository.HasOverlapAsync(
                    schedule.DoctorId,
                    schedule.DayOfWeek,
                    schedule.StartTime,
                    schedule.EndTime,
                    schedule.DoctorScheduleId);

            if (hasOverlap)
                return false;

            var oldValues = SerializeSchedule(schedule);

            schedule.IsAvailable = true;

            doctorScheduleRepository.Update(schedule);

            await doctorScheduleRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Activate",
                schedule,
                oldValues);

            return true;
        }

        public async Task<bool> UpdateAsync(
            DoctorSchedule schedule)
        {
            var existingSchedule =
                await doctorScheduleRepository.GetByIdAsync(
                    schedule.DoctorScheduleId);

            if (existingSchedule == null)
                return false;

            bool doctorExists =
                await doctorScheduleRepository.DoctorExistsAsync(
                    schedule.DoctorId);

            if (!doctorExists)
                return false;

            if (schedule.StartTime >= schedule.EndTime)
                return false;

            bool hasDuplicateStartTime =
                await doctorScheduleRepository.HasDuplicateStartTimeAsync(
                    schedule.DoctorId,
                    schedule.DayOfWeek,
                    schedule.StartTime,
                    schedule.DoctorScheduleId);

            if (hasDuplicateStartTime)
                return false;

            bool hasOverlap =
                await doctorScheduleRepository.HasOverlapAsync(
                    schedule.DoctorId,
                    schedule.DayOfWeek,
                    schedule.StartTime,
                    schedule.EndTime,
                    schedule.DoctorScheduleId);

            if (hasOverlap)
                return false;

            var oldValues = SerializeSchedule(existingSchedule);

            doctorScheduleRepository.Update(schedule);

            await doctorScheduleRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Update",
                schedule,
                oldValues);

            return true;
        }

        public async Task<List<DoctorSchedule>> GetAllAsync()
        {
            return await doctorScheduleRepository.GetAllAsync();
        }

        public async Task<List<DoctorSchedule>> GetByDoctorIdAsync(
            int doctorId)
        {
            return await doctorScheduleRepository
                .GetByDoctorIdAsync(doctorId);
        }

        public async Task<DoctorSchedule?> GetByIdAsync(int id)
        {
            return await doctorScheduleRepository.GetByIdAsync(id);
        }

        public async Task<(List<DoctorSchedule> Schedules, int TotalCount)>
            FilterAsync(
                string? search,
                int? doctorId,
                DayOfWeek? dayOfWeek,
                bool? isAvailable,
                int page,
                int pageSize)
        {
            return await doctorScheduleRepository.FilterAsync(
                search,
                doctorId,
                dayOfWeek,
                isAvailable,
                page,
                pageSize);
        }

        public async Task DeactivateByDoctorIdAsync(int doctorId)
        {
            await doctorScheduleRepository
                .DeactivateByDoctorIdAsync(doctorId);
        }

        public async Task ActivateByDoctorIdAsync(int doctorId)
        {
            await doctorScheduleRepository
                .ActivateByDoctorIdAsync(doctorId);
        }

        private async Task WriteAuditLogAsync(
            string action,
            DoctorSchedule schedule,
            string? oldValues)
        {
            var userId = await GetCurrentUserIdAsync();

            if (!userId.HasValue)
                return;

            await auditLogService.LogAsync(
                action: action,
                entityName: nameof(DoctorSchedule),
                entityId: schedule.DoctorScheduleId,
                userId: userId.Value,
                oldValues: oldValues,
                newValues: SerializeSchedule(schedule),
                ipAddress: GetIpAddress());
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var userIdClaim =
                httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            var user =
                await userManager.FindByIdAsync(userIdClaim);

            return user?.Id;
        }

        private string? GetIpAddress()
        {
            return httpContextAccessor.HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();
        }

        private string SerializeSchedule(
            DoctorSchedule schedule)
        {
            return JsonSerializer.Serialize(new
            {
                schedule.DoctorScheduleId,
                schedule.DoctorId,
                schedule.DayOfWeek,
                schedule.StartTime,
                schedule.EndTime,
                schedule.IsAvailable
            });
        }
    }
}