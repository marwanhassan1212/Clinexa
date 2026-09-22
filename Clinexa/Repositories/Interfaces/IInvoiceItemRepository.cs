using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IInvoiceItemRepository
    {
        Task<InvoiceItem?> GetByIdAsync(int id);

        Task<List<InvoiceItem>> GetAllAsync();

        Task<List<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId);

        Task<bool> InvoiceExistsAsync(int invoiceId);

        Task AddAsync(InvoiceItem invoiceItem);

        void Update(InvoiceItem invoiceItem);

        void Delete(InvoiceItem invoiceItem);
        Task SaveChangesAsync();
    }
}
