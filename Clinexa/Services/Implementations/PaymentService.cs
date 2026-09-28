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
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository paymentRepository;

        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            this.paymentRepository = paymentRepository;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }

        public async Task<bool> CreateAsync(Payment payment)
        {
            // 1. Invoice must exist
            bool invoiceExists =
                await paymentRepository.InvoiceExistsAsync(
                    payment.InvoiceId);

            if (!invoiceExists)
                return false;

            // 2. Payment amount must be positive
            if (payment.Amount <= 0)
                return false;

            // 3. Get invoice
            var invoice =
                await paymentRepository.GetInvoiceByIdAsync(
                    payment.InvoiceId);

            if (invoice == null)
                return false;

            if (invoice.InvoiceStatus == InvoiceStatus.Cancelled)
                return false;

            // 4. Get previous payments
            decimal totalPaid =
                await paymentRepository
                    .GetTotalPaidForInvoiceAsync(
                        payment.InvoiceId);

            // 5. Calculate remaining amount
            decimal remainingAmount =
                invoice.TotalAmount - totalPaid;

            // 6. Invoice is already fully paid
            if (remainingAmount <= 0)
                return false;

            // 7. Payment cannot exceed remaining amount
            if (payment.Amount > remainingAmount)
                return false;

            // 8. Set payment date automatically
            if (payment.PaymentDate == default)
                payment.PaymentDate = DateTime.Now;

            // 9. Add payment
            await paymentRepository.AddAsync(payment);

            // 10. Recalculate invoice financial data
            totalPaid += payment.Amount;

            invoice.PaidAmount = totalPaid;

            invoice.RemainingAmount =
                invoice.TotalAmount - totalPaid;

            // 11. Update invoice status
            if (invoice.PaidAmount == 0)
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.Unpaid;
            }
            else if (invoice.PaidAmount < invoice.TotalAmount)
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.Paid;
            }

            // 12. Save payment + invoice together
            await paymentRepository.SaveChangesAsync();

            // 13. Audit
            await WriteAuditLogAsync(
                "Create",
                payment,
                null);

            return true;
        }

        public async Task<(List<Payment> Payments, int TotalCount)>
            FilterAsync(
                string? search,
                int? invoiceId,
                PaymentMethod? paymentMethod,
                DateTime? paymentDateFrom,
                DateTime? paymentDateTo,
                decimal? minAmount,
                decimal? maxAmount,
                int page,
                int pageSize)
        {
            return await paymentRepository.FilterAsync(
                search,
                invoiceId,
                paymentMethod,
                paymentDateFrom,
                paymentDateTo,
                minAmount,
                maxAmount,
                page,
                pageSize);
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await paymentRepository.GetAllAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await paymentRepository.GetByIdAsync(id);
        }

        public async Task<List<Payment>> GetByInvoiceIdAsync(
            int invoiceId)
        {
            return await paymentRepository
                .GetByInvoiceIdAsync(invoiceId);
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            // 1. Get existing payment
            var existingPayment =
                await paymentRepository
                    .GetByIdAsync(payment.PaymentId);

            if (existingPayment == null)
                return false;

            // 2. Payment cannot be moved
            // to another invoice
            if (payment.InvoiceId != existingPayment.InvoiceId)
                return false;

            // 3. Invoice must exist
            bool invoiceExists =
                await paymentRepository
                    .InvoiceExistsAsync(payment.InvoiceId);

            if (!invoiceExists)
                return false;

            // 4. Payment amount must be positive
            if (payment.Amount <= 0)
                return false;

            // 5. Get invoice
            var invoice =
                await paymentRepository
                    .GetInvoiceByIdAsync(payment.InvoiceId);

            if (invoice == null)
                return false;

            if (invoice.InvoiceStatus == InvoiceStatus.Cancelled)
                return false;

            // 6. Get all payments total
            decimal totalPaid =
                await paymentRepository
                    .GetTotalPaidForInvoiceAsync(
                        payment.InvoiceId);

            // 7. Remove old payment amount
            totalPaid -= existingPayment.Amount;

            // 8. Calculate remaining amount
            decimal remainingAmount =
                invoice.TotalAmount - totalPaid;

            // 9. New payment cannot exceed remaining amount
            if (payment.Amount > remainingAmount)
                return false;

            // 10. Preserve payment date
            if (payment.PaymentDate == default)
            {
                payment.PaymentDate =
                    existingPayment.PaymentDate;
            }

            // 11. Calculate new total
            totalPaid += payment.Amount;

            // 12. Update invoice
            invoice.PaidAmount = totalPaid;

            invoice.RemainingAmount =
                invoice.TotalAmount - totalPaid;

            // 13. Update invoice status
            if (invoice.PaidAmount == 0)
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.Unpaid;
            }
            else if (invoice.PaidAmount < invoice.TotalAmount)
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.Paid;
            }

            // Capture old values before update
            var oldValues =
                SerializePayment(existingPayment);

            // 14. Update payment
            paymentRepository.Update(payment);

            // 15. Save everything
            await paymentRepository.SaveChangesAsync();

            // 16. Audit
            await WriteAuditLogAsync(
                "Update",
                payment,
                oldValues);

            return true;
        }

        private async Task WriteAuditLogAsync(
            string action,
            Payment payment,
            string? oldValues)
        {
            var userId =
                await GetCurrentUserIdAsync();

            if (!userId.HasValue)
                return;

            await auditLogService.LogAsync(
                action: action,
                entityName: nameof(Payment),
                entityId: payment.PaymentId,
                userId: userId.Value,
                oldValues: oldValues,
                newValues: SerializePayment(payment),
                ipAddress: GetIpAddress());
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

        private string SerializePayment(
            Payment payment)
        {
            return JsonSerializer.Serialize(new
            {
                payment.PaymentId,
                payment.InvoiceId,
                payment.Amount,
                payment.PaymentMethod,
                payment.PaymentDate
            });
        }
    }
}