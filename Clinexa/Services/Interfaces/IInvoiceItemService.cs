using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IInvoiceItemService
    {
        Task<bool> CreateAsync(InvoiceItem invoiceItem);

        Task<List<InvoiceItem>> GetAllAsync();

        Task<InvoiceItem?> GetByIdAsync(int id);

        Task<List<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId);

        Task<bool> UpdateAsync(InvoiceItem invoiceItem);
    }
}
