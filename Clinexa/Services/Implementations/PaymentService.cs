using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository paymentRepository;
        public PaymentService(IPaymentRepository paymentRepository)
        {
            this.paymentRepository = paymentRepository;
        }
        public async Task<bool> CreateAsync(Payment payment)
        {
            // 1. Invoice must exist
            bool invoiceExists =
                await paymentRepository.InvoiceExistsAsync(payment.InvoiceId);

            if (!invoiceExists)
                return false;

            // 2. Payment amount must be positive
            if (payment.Amount <= 0)
                return false;

            // 3. Get invoice
            var invoice =
                await paymentRepository.GetInvoiceByIdAsync(payment.InvoiceId);

            if (invoice == null)
                return false;

            // 4. Get previous payments
            decimal totalPaid =
                await paymentRepository.GetTotalPaidForInvoiceAsync(payment.InvoiceId);

            // 5. Calculate remaining amount
            decimal remainingAmount =
                invoice.TotalAmount - totalPaid;

            // 6. Payment cannot exceed remaining amount
            if (payment.Amount > remainingAmount)
                return false;

            // 7. Set payment date automatically if not provided
            if (payment.PaymentDate == default)
                payment.PaymentDate = DateTime.Now;

            // 8. Add payment
            await paymentRepository.AddAsync(payment);

            // 9. Recalculate invoice financial data
            totalPaid += payment.Amount;

            invoice.PaidAmount = totalPaid;
            invoice.RemainingAmount =
                invoice.TotalAmount - totalPaid;

            // 10. Update invoice status
            if (invoice.PaidAmount == 0)
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.Unpaid;
            }
            else if (invoice.PaidAmount < invoice.TotalAmount)
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.Paid;
            }

            // 11. Save payment + invoice together
            await paymentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<(List<Payment> Payments, int TotalCount)> FilterAsync(
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

        public async Task<List<Payment>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await paymentRepository.GetByInvoiceIdAsync(invoiceId);
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            // 1. Get existing payment
            var existingPayment =
                await paymentRepository.GetByIdAsync(payment.PaymentId);

            if (existingPayment == null)
                return false;

            // 2. Payment cannot be moved to another invoice
            if (payment.InvoiceId != existingPayment.InvoiceId)
                return false;

            // 3. Invoice must exist
            bool invoiceExists =
                await paymentRepository.InvoiceExistsAsync(payment.InvoiceId);

            if (!invoiceExists)
                return false;

            // 4. Payment amount must be positive
            if (payment.Amount <= 0)
                return false;

            // 5. Get invoice
            var invoice =
                await paymentRepository.GetInvoiceByIdAsync(payment.InvoiceId);

            if (invoice == null)
                return false;

            // 6. Get all payments total
            decimal totalPaid =
                await paymentRepository.GetTotalPaidForInvoiceAsync(payment.InvoiceId);

            // 7. Remove old payment amount from calculation
            totalPaid -= existingPayment.Amount;

            // 8. Calculate remaining amount
            decimal remainingAmount =
                invoice.TotalAmount - totalPaid;

            // 9. New payment cannot exceed remaining amount
            if (payment.Amount > remainingAmount)
                return false;

            // 10. Preserve payment date if not supplied
            if (payment.PaymentDate == default)
                payment.PaymentDate = existingPayment.PaymentDate;

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
                    Enums.InvoiceStatus.Unpaid;
            }
            else if (invoice.PaidAmount < invoice.TotalAmount)
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.Paid;
            }

            // 14. Update payment
            paymentRepository.Update(payment);

            // 15. Save everything
            await paymentRepository.SaveChangesAsync();

            return true;
        }
    }
}
