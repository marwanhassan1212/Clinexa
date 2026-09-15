using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class InvoiceItemService : IInvoiceItemService
    {
        private readonly IInvoiceItemRepository invoiceItemRepository;
        public InvoiceItemService(IInvoiceItemRepository invoiceItemRepository)
        {
            this.invoiceItemRepository = invoiceItemRepository;
        }
        public async Task<bool> CreateAsync(InvoiceItem invoiceItem)
        {
          
            bool invoiceExists =
                await invoiceItemRepository
                    .InvoiceExistsAsync(invoiceItem.InvoiceId);

            if (!invoiceExists)
            {
                return false;
            }

            
            if (invoiceItem.Quantity <= 0)
            {
                return false;
            }

            
            if (invoiceItem.UnitPrice < 0)
            {
                return false;
            }

            
            invoiceItem.TotalPrice =
                invoiceItem.Quantity *
                invoiceItem.UnitPrice;

            await invoiceItemRepository
                .AddAsync(invoiceItem);

            await invoiceItemRepository
                .SaveChangesAsync();

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

        public async Task<List<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await invoiceItemRepository.GetByInvoiceIdAsync(invoiceId);
        }

        public async Task<bool> UpdateAsync(InvoiceItem invoiceItem)
        {
            
            var existingInvoiceItem =
                await invoiceItemRepository
                    .GetByIdAsync(invoiceItem.InvoiceItemId);

            if (existingInvoiceItem == null)
            {
                return false;
            }

            
            bool invoiceExists =
                await invoiceItemRepository
                    .InvoiceExistsAsync(invoiceItem.InvoiceId);

            if (!invoiceExists)
            {
                return false;
            }

           
            if (invoiceItem.Quantity <= 0)
            {
                return false;
            }

            
            if (invoiceItem.UnitPrice < 0)
            {
                return false;
            }

            
            invoiceItem.TotalPrice =
                invoiceItem.Quantity *
                invoiceItem.UnitPrice;

            invoiceItemRepository
                .Update(invoiceItem);

            await invoiceItemRepository
                .SaveChangesAsync();

            return true;
        }
    }
}
