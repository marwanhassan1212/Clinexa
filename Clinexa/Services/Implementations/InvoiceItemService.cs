using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;

namespace Clinexa.Services.Implementations
{
    public class InvoiceItemService : IInvoiceItemService
    {
        private readonly IInvoiceItemRepository invoiceItemRepository;
        private readonly IInvoiceService invoiceService;

        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;

        public InvoiceItemService(
            IInvoiceItemRepository invoiceItemRepository,
            IInvoiceService invoiceService,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            this.invoiceItemRepository = invoiceItemRepository;
            this.invoiceService = invoiceService;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }

        public async Task<bool> CreateAsync(InvoiceItem invoiceItem)
        {
            bool invoiceExists =
                await invoiceItemRepository
                    .InvoiceExistsAsync(invoiceItem.InvoiceId);

            if (!invoiceExists)
                return false;

            if (invoiceItem.Quantity <= 0)
                return false;

            if (invoiceItem.UnitPrice < 0)
                return false;

            invoiceItem.TotalPrice =
                invoiceItem.Quantity * invoiceItem.UnitPrice;

            await invoiceItemRepository.AddAsync(invoiceItem);
            await invoiceItemRepository.SaveChangesAsync();

            bool result =
                await RecalculateInvoiceAsync(
                    invoiceItem.InvoiceId);

            if (!result)
                return false;

            await WriteAuditLogAsync(
                "Create",
                invoiceItem,
                null);

            return true;
        }

        public async Task<List<InvoiceItem>> GetAllAsync()
        {
            return await invoiceItemRepository.GetAllAsync();
        }

        public async Task<InvoiceItem?> GetByIdAsync(int id)
        {
            return await invoiceItemRepository.GetByIdAsync(id);
        }

        public async Task<List<InvoiceItem>> GetByInvoiceIdAsync(
            int invoiceId)
        {
            return await invoiceItemRepository
                .GetByInvoiceIdAsync(invoiceId);
        }

        public async Task<bool> UpdateAsync(
            InvoiceItem invoiceItem)
        {
            var existingInvoiceItem =
                await invoiceItemRepository
                    .GetByIdAsync(invoiceItem.InvoiceItemId);

            if (existingInvoiceItem == null)
                return false;

            if (existingInvoiceItem.InvoiceId != invoiceItem.InvoiceId)
                return false;

            bool invoiceExists =
                await invoiceItemRepository
                    .InvoiceExistsAsync(invoiceItem.InvoiceId);

            if (!invoiceExists)
                return false;

            if (invoiceItem.Quantity <= 0)
                return false;

            if (invoiceItem.UnitPrice < 0)
                return false;

            invoiceItem.TotalPrice =
                invoiceItem.Quantity * invoiceItem.UnitPrice;

            var oldValues =
                SerializeInvoiceItem(existingInvoiceItem);

            invoiceItemRepository.Update(invoiceItem);

            await invoiceItemRepository.SaveChangesAsync();

            bool result =
                await RecalculateInvoiceAsync(
                    invoiceItem.InvoiceId);

            if (!result)
                return false;

            await WriteAuditLogAsync(
                "Update",
                invoiceItem,
                oldValues);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoiceItem =
                await invoiceItemRepository
                    .GetByIdAsync(id);

            if (invoiceItem == null)
                return false;

            int invoiceId = invoiceItem.InvoiceId;

            var oldValues =
                SerializeInvoiceItem(invoiceItem);

            invoiceItemRepository.Delete(invoiceItem);

            await invoiceItemRepository.SaveChangesAsync();

            bool result =
                await RecalculateInvoiceAsync(invoiceId);

            if (!result)
                return false;

            await WriteAuditLogAsync(
                "Delete",
                invoiceItem,
                oldValues);

            return true;
        }

        private async Task<bool> RecalculateInvoiceAsync(
            int invoiceId)
        {
            var invoice =
                await invoiceService.GetByIdAsync(invoiceId);

            if (invoice == null)
                return false;

            var items =
                await invoiceItemRepository
                    .GetByInvoiceIdAsync(invoiceId);

            invoice.SubTotal =
                items.Sum(x => x.TotalPrice);

            return await invoiceService.UpdateAsync(invoice);
        }

        public async Task<bool> InvoiceExistsAsync(
            int invoiceId)
        {
            return await invoiceItemRepository
                .InvoiceExistsAsync(invoiceId);
        }

        private async Task WriteAuditLogAsync(
            string action,
            InvoiceItem invoiceItem,
            string? oldValues)
        {
            var userId = await GetCurrentUserIdAsync();

            if (!userId.HasValue)
                return;

            await auditLogService.LogAsync(
                action: action,
                entityName: nameof(InvoiceItem),
                entityId: invoiceItem.InvoiceItemId,
                userId: userId.Value,
                oldValues: oldValues,
                newValues: action == "Delete"
                    ? null
                    : SerializeInvoiceItem(invoiceItem),
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

        private string SerializeInvoiceItem(
            InvoiceItem invoiceItem)
        {
            return JsonSerializer.Serialize(new
            {
                invoiceItem.InvoiceItemId,
                invoiceItem.InvoiceId,
                invoiceItem.Description,
                invoiceItem.Quantity,
                invoiceItem.UnitPrice,
                invoiceItem.TotalPrice
            });
        }
    }
}