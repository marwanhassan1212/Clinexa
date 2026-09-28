using System.Security.Claims;
using System.Text.Json;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Clinexa.Services.Implementations
{
    public class PrescriptionItemService : IPrescriptionItemService
    {
        private readonly IPrescriptionItemRepository prescriptionItemRepository;
        private readonly UserManager<User> userManager;
        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public PrescriptionItemService(
            IPrescriptionItemRepository prescriptionItemRepository,
            UserManager<User> userManager,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.prescriptionItemRepository = prescriptionItemRepository;
            this.userManager = userManager;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
        }

        // =========================================================
        // Data-Level Authorization
        // =========================================================

        public async Task<bool> CanAccessAsync(
            int prescriptionItemId,
            int currentUserId)
        {
            var user =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (user == null)
            {
                return false;
            }

            // Admin can access everything.
            if (await userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                return true;
            }

            // Doctor can access only items
            // belonging to their own prescription.
            return await prescriptionItemRepository
                .BelongsToDoctorAsync(
                    prescriptionItemId,
                    currentUserId);
        }

        public async Task<bool>
            CanAccessPrescriptionAsync(
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

            // Admin can access everything.
            if (await userManager.IsInRoleAsync(
                    user,
                    "Admin"))
            {
                return true;
            }

            // Doctor can create items only inside
            // their own prescription.
            return await prescriptionItemRepository
                .PrescriptionBelongsToDoctorAsync(
                    prescriptionId,
                    currentUserId);
        }

        // =========================================================
        // Create
        // =========================================================

        public async Task<bool> CreateAsync(
            PrescriptionItem prescriptionItem)
        {
            bool medicineAlreadyExists =
                await prescriptionItemRepository
                    .ExistsForPrescriptionAsync(
                        prescriptionItem.PrescriptionId,
                        prescriptionItem.MedicineId);

            if (medicineAlreadyExists)
            {
                return false;
            }

            bool prescriptionExists =
                await prescriptionItemRepository
                    .PrescriptionExistsAsync(
                        prescriptionItem.PrescriptionId);

            if (!prescriptionExists)
            {
                return false;
            }

            bool activeMedicineExists =
                await prescriptionItemRepository
                    .ActiveMedicineExistsAsync(
                        prescriptionItem.MedicineId);

            if (!activeMedicineExists)
            {
                return false;
            }

            await prescriptionItemRepository
                .AddAsync(prescriptionItem);

            await prescriptionItemRepository
                .SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
                {
                    prescriptionItem.PrescriptionItemId,
                    prescriptionItem.PrescriptionId,
                    prescriptionItem.MedicineId
                });

                await auditLogService.LogAsync(
                    "Create",
                    "PrescriptionItem",
                    prescriptionItem.PrescriptionItemId,
                    userId.Value,
                    newValues: newValues,
                    ipAddress: GetIpAddress());
            }

            return true;
        }

        // =========================================================
        // Get
        // =========================================================

        public async Task<List<PrescriptionItem>> GetAllAsync()
        {
            return await prescriptionItemRepository
                .GetAllAsync();
        }

        public async Task<PrescriptionItem?> GetByIdAsync(
            int id)
        {
            return await prescriptionItemRepository
                .GetByIdAsync(id);
        }

        public async Task<List<PrescriptionItem>>
            GetByPrescriptionIdAsync(
                int prescriptionId)
        {
            return await prescriptionItemRepository
                .GetByPrescriptionIdAsync(
                    prescriptionId);
        }

        // =========================================================
        // Update
        // =========================================================

        public async Task<bool> UpdateAsync(
            PrescriptionItem prescriptionItem)
        {
            var prescriptionItemExists =
                await prescriptionItemRepository
                    .GetByIdAsync(
                        prescriptionItem.PrescriptionItemId);

            if (prescriptionItemExists == null)
            {
                return false;
            }

            bool prescriptionExists =
                await prescriptionItemRepository
                    .PrescriptionExistsAsync(
                        prescriptionItem.PrescriptionId);

            if (!prescriptionExists)
            {
                return false;
            }

            bool activeMedicineExists =
                await prescriptionItemRepository
                    .ActiveMedicineExistsAsync(
                        prescriptionItem.MedicineId);

            if (!activeMedicineExists)
            {
                return false;
            }

            bool medicineAlreadyExists =
                await prescriptionItemRepository
                    .ExistsForPrescriptionAsync(
                        prescriptionItem.PrescriptionId,
                        prescriptionItem.MedicineId,
                        prescriptionItem.PrescriptionItemId);

            if (medicineAlreadyExists)
            {
                return false;
            }

            var oldValues = JsonSerializer.Serialize(new
            {
                prescriptionItemExists.PrescriptionItemId,
                prescriptionItemExists.PrescriptionId,
                prescriptionItemExists.MedicineId
            });

            prescriptionItemRepository
                .Update(prescriptionItem);

            await prescriptionItemRepository
                .SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();

            if (userId.HasValue)
            {
                var newValues = JsonSerializer.Serialize(new
                {
                    prescriptionItem.PrescriptionItemId,
                    prescriptionItem.PrescriptionId,
                    prescriptionItem.MedicineId
                });

                await auditLogService.LogAsync(
                    "Update",
                    "PrescriptionItem",
                    prescriptionItem.PrescriptionItemId,
                    userId.Value,
                    oldValues,
                    newValues,
                    GetIpAddress());
            }

            return true;
        }

        // =========================================================
        // Audit Log Helpers
        // =========================================================

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var userIdClaim =
                httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

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