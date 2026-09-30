using System.Security.Claims;
using System.Text.Json;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Clinexa.Services.Implementations
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository medicineRepository;
        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;
        private readonly IDateTimeService dateTimeService;

        public MedicineService(
            IMedicineRepository medicineRepository,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IDateTimeService dateTimeService)
        {
            this.medicineRepository = medicineRepository;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.dateTimeService = dateTimeService;
        }

        public async Task<bool> CreateAsync(Medicine medicine)
        {
            bool nameExists =
                await medicineRepository
                    .ExistsByNameAsync(medicine.Name);

            if (nameExists)
            {
                return false;
            }

            medicine.IsActive = true;
            medicine.CreatedAt = dateTimeService.Now;
            medicine.Name = medicine.Name.Trim();

            if (!string.IsNullOrWhiteSpace(medicine.GenericName))
                medicine.GenericName = medicine.GenericName.Trim();

            if (!string.IsNullOrWhiteSpace(medicine.Description))
                medicine.Description = medicine.Description.Trim();

            await medicineRepository.AddAsync(medicine);
            await medicineRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
                {
                    medicine.MedicineId,
                    medicine.Name,
                    medicine.GenericName,
                    medicine.Description,
                    medicine.IsActive
                });

                await auditLogService.LogAsync(
                    "Create",
                    "Medicine",
                    medicine.MedicineId,
                    userId.Value,
                    newValues: newValues,
                    ipAddress: GetIpAddress());
            }

            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var medicine =
                await medicineRepository
                    .GetByIdAsync(id);

            if (medicine == null)
            {
                return false;
            }

            var oldValues = JsonSerializer.Serialize(new
            {
                medicine.MedicineId,
                medicine.Name,
                medicine.GenericName,
                medicine.Description,
                medicine.IsActive
            });

            medicine.IsActive = false;

            medicineRepository.Update(medicine);

            await medicineRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
                {
                    medicine.MedicineId,
                    medicine.Name,
                    medicine.GenericName,
                    medicine.Description,
                    medicine.IsActive
                });

                await auditLogService.LogAsync(
                    "Deactivate",
                    "Medicine",
                    medicine.MedicineId,
                    userId.Value,
                    oldValues,
                    newValues,
                    GetIpAddress());
            }

            return true;
        }

        public async Task<List<Medicine>> GetActiveAsync()
        {
            return await medicineRepository.GetActiveAsync();
        }

        public async Task<List<Medicine>> GetAllAsync()
        {
            return await medicineRepository.GetAllAsync();
        }

        public async Task<Medicine?> GetByIdAsync(int id)
        {
            return await medicineRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(Medicine medicine)
        {
            var medicineExists =
                await medicineRepository
                    .GetByIdAsync(medicine.MedicineId);

            if (medicineExists == null)
            {
                return false;
            }

            bool nameExists =
                await medicineRepository
                    .ExistsByNameAsync(
                        medicine.Name,
                        medicine.MedicineId);

            if (nameExists)
            {
                return false;
            }

            var oldValues = JsonSerializer.Serialize(new
            {
                medicineExists.MedicineId,
                medicineExists.Name,
                medicineExists.GenericName,
                medicineExists.Description,
                medicineExists.IsActive
            });

            medicine.Name = medicine.Name.Trim();

            if (!string.IsNullOrWhiteSpace(medicine.GenericName))
                medicine.GenericName = medicine.GenericName.Trim();

            if (!string.IsNullOrWhiteSpace(medicine.Description))
                medicine.Description = medicine.Description.Trim();

            medicineRepository.Update(medicine);

            await medicineRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
                {
                    medicine.MedicineId,
                    medicine.Name,
                    medicine.GenericName,
                    medicine.Description,
                    medicine.IsActive
                });

                await auditLogService.LogAsync(
                    "Update",
                    "Medicine",
                    medicine.MedicineId,
                    userId.Value,
                    oldValues,
                    newValues,
                    GetIpAddress());
            }

            return true;
        }

        public async Task<(List<Medicine> Medicines, int TotalCount)> FilterAsync(
            string? search,
            bool? isActive,
            int page,
            int pageSize)
        {
            return await medicineRepository.FilterAsync(
                search,
                isActive,
                page,
                pageSize);
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var medicine =
                await medicineRepository
                    .GetByIdAsync(id);

            if (medicine == null)
            {
                return false;
            }

            var oldValues = JsonSerializer.Serialize(new
            {
                medicine.MedicineId,
                medicine.Name,
                medicine.GenericName,
                medicine.Description,
                medicine.IsActive
            });

            medicine.IsActive = true;

            medicineRepository.Update(medicine);

            await medicineRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
                {
                    medicine.MedicineId,
                    medicine.Name,
                    medicine.GenericName,
                    medicine.Description,
                    medicine.IsActive
                });

                await auditLogService.LogAsync(
                    "Activate",
                    "Medicine",
                    medicine.MedicineId,
                    userId.Value,
                    oldValues,
                    newValues,
                    GetIpAddress());
            }

            return true;
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var userIdClaim =
                httpContextAccessor.HttpContext?
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
    }
}