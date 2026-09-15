using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> CreateAsync(Payment payment);

        Task<List<Payment>> GetAllAsync();

        Task<Payment?> GetByIdAsync(int id);

        Task<List<Payment>> GetByInvoiceIdAsync(int invoiceId);

        Task<bool> UpdateAsync(Payment payment);
    }
}
