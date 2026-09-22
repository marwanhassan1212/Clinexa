using Clinexa.Enums;
using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id);

        Task<List<Payment>> GetAllAsync();

        Task<List<Payment>> GetByInvoiceIdAsync(int invoiceId);

        Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);

        Task<bool> InvoiceExistsAsync(int invoiceId);
        Task<(List<Payment> Payments, int TotalCount)> FilterAsync(
                string? search,
                int? invoiceId,
                PaymentMethod? paymentMethod,
                DateTime? paymentDateFrom,
                DateTime? paymentDateTo,
                decimal? minAmount,
                decimal? maxAmount,
                int page,
                int pageSize);

        Task<decimal> GetTotalPaidForInvoiceAsync(int invoiceId);

        Task AddAsync(Payment payment);

        void Update(Payment payment);

        Task SaveChangesAsync();
    }
}
