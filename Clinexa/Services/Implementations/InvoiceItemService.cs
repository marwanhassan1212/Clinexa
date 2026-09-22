using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class InvoiceItemService : IInvoiceItemService
    {
        private readonly IInvoiceItemRepository invoiceItemRepository;
        private readonly IInvoiceService invoiceService;

        public InvoiceItemService(
            IInvoiceItemRepository invoiceItemRepository,
            IInvoiceService invoiceService)
        {
            this.invoiceItemRepository = invoiceItemRepository;
            this.invoiceService = invoiceService;
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
                invoiceItem.Quantity * invoiceItem.UnitPrice;

            await invoiceItemRepository.AddAsync(invoiceItem);
            await invoiceItemRepository.SaveChangesAsync();

            return await RecalculateInvoiceAsync(
                invoiceItem.InvoiceId);
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

        public async Task<bool> UpdateAsync(InvoiceItem invoiceItem)
        {
            var existingInvoiceItem =
                await invoiceItemRepository
                    .GetByIdAsync(invoiceItem.InvoiceItemId);

            if (existingInvoiceItem == null)
            {
                return false;
            }

            // An item cannot be moved between invoices.
            if (existingInvoiceItem.InvoiceId != invoiceItem.InvoiceId)
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
                invoiceItem.Quantity * invoiceItem.UnitPrice;

            invoiceItemRepository.Update(invoiceItem);
            await invoiceItemRepository.SaveChangesAsync();

            return await RecalculateInvoiceAsync(
                invoiceItem.InvoiceId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoiceItem =
                await invoiceItemRepository.GetByIdAsync(id);

            if (invoiceItem == null)
            {
                return false;
            }

            int invoiceId = invoiceItem.InvoiceId;

            invoiceItemRepository.Delete(invoiceItem);
            await invoiceItemRepository.SaveChangesAsync();

            return await RecalculateInvoiceAsync(invoiceId);
        }

        private async Task<bool> RecalculateInvoiceAsync(int invoiceId)
        {
            var invoice =
                await invoiceService.GetByIdAsync(invoiceId);

            if (invoice == null)
            {
                return false;
            }

            var items =
                await invoiceItemRepository
                    .GetByInvoiceIdAsync(invoiceId);

            invoice.SubTotal =
                items.Sum(x => x.TotalPrice);

            return await invoiceService.UpdateAsync(invoice);
        }

        public async Task<bool> InvoiceExistsAsync(int invoiceId)
        {
            return await invoiceItemRepository
                .InvoiceExistsAsync(invoiceId);
        }
    }
}