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

        Task<decimal> GetTotalPaidForInvoiceAsync(int invoiceId);

        Task AddAsync(Payment payment);

        void Update(Payment payment);

        Task SaveChangesAsync();
    }
}
