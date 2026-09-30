using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;

namespace Clinexa.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository doctorRepository;
        private readonly IDoctorScheduleRepository doctorScheduleRepository;
        private readonly IDateTimeService dateTimeService;
        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;

        public DoctorService(
            IDoctorRepository doctorRepository,
            IDoctorScheduleRepository doctorScheduleRepository,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IDateTimeService dateTimeService)
        {
            this.doctorRepository = doctorRepository;
            this.doctorScheduleRepository = doctorScheduleRepository;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.dateTimeService = dateTimeService;
        }

        public async Task<bool> CreateAsync(Doctor doctor)
        {
            bool userExists =
                await doctorRepository.ExistsByUserId(doctor.UserId);

            if (userExists)
                return false;

            bool specialityExists =
                await doctorRepository.ExistBySpecialityId(doctor.SpecialityId);

            if (!specialityExists)
                return false;

            if (doctor.ConsultationFee < 0)
                return false;

            doctor.CreatedAt = dateTimeService.Now;
            doctor.IsActive = true;

            await doctorRepository.AddAsync(doctor);
            await doctorRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                action: "Create",
                doctor: doctor,
                oldValues: null);

            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var doctor = await doctorRepository.GetByIdAsync(id);

            if (doctor == null)
                return false;

            var oldValues = SerializeDoctor(doctor);

            doctor.IsActive = false;

            doctorRepository.UpdateAsync(doctor);

            await doctorScheduleRepository.DeactivateByDoctorIdAsync(
                doctor.DoctorId);

            await doctorRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                action: "Deactivate",
                doctor: doctor,
                oldValues: oldValues);

            return true;
        }

        public async Task<List<Doctor>> GetAllAsync()
        {
            return await doctorRepository.GetAllAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await doctorRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(Doctor doctor)
        {
            var doctorExists =
                await doctorRepository.GetByIdAsync(doctor.DoctorId);

            if (doctorExists == null)
                return false;

            if (doctor.ConsultationFee < 0)
                return false;

            bool specialityExists =
                await doctorRepository.ExistBySpecialityId(
                    doctor.SpecialityId);

            if (!specialityExists)
                return false;

            var oldValues = SerializeDoctor(doctorExists);

            doctorExists.SpecialityId = doctor.SpecialityId;
            doctorExists.ConsultationFee = doctor.ConsultationFee;

            doctorRepository.UpdateAsync(doctorExists);

            await doctorRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                action: "Update",
                doctor: doctorExists,
                oldValues: oldValues);

            return true;
        }

        public async Task<(List<Doctor> Doctors, int TotalCount)> FilterAsync(
            string? search,
            int? specialityId,
            bool? isActive,
            int page,
            int pageSize)
        {
            return await doctorRepository.FilterAsync(
                search,
                specialityId,
                isActive,
                page,
                pageSize);
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var doctor = await doctorRepository.GetByIdAsync(id);

            if (doctor == null)
                return false;

            var oldValues = SerializeDoctor(doctor);

            doctor.IsActive = true;

            doctorRepository.UpdateAsync(doctor);

            await doctorScheduleRepository.ActivateByDoctorIdAsync(
                doctor.DoctorId);

            await doctorRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                action: "Activate",
                doctor: doctor,
                oldValues: oldValues);

            return true;
        }

        public async Task<List<User>> GetAvailableUsersAsync()
        {
            return await doctorRepository.GetAvailableUsersAsync();
        }

        private async Task WriteAuditLogAsync(
            string action,
            Doctor doctor,
            string? oldValues)
        {
            var userId = await GetCurrentUserIdAsync();

            if (!userId.HasValue)
                return;

            var newValues = SerializeDoctor(doctor);

            await auditLogService.LogAsync(
                action: action,
                entityName: nameof(Doctor),
                entityId: doctor.DoctorId,
                userId: userId.Value,
                oldValues: oldValues,
                newValues: newValues,
                ipAddress: GetIpAddress());
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var userIdClaim = httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            var user = await userManager.FindByIdAsync(userIdClaim);

            return user?.Id;
        }

        private string? GetIpAddress()
        {
            return httpContextAccessor.HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();
        }

        private string SerializeDoctor(Doctor doctor)
        {
            return JsonSerializer.Serialize(new
            {
                doctor.DoctorId,
                doctor.UserId,
                doctor.SpecialityId,
                doctor.ConsultationFee,
                doctor.IsActive,
                doctor.CreatedAt
            });
        }
    }
}