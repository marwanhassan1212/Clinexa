using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class InvoiceItemRepository : IInvoiceItemRepository
    {
        private readonly AppDbContext _db;
        public InvoiceItemRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(InvoiceItem invoiceItem)
        {
             await _db.InvoiceItems.AddAsync(invoiceItem);
        }

        public async Task<List<InvoiceItem>> GetAllAsync()
        {
            return await _db.InvoiceItems
                .AsNoTracking()
                .OrderByDescending(x => x.InvoiceItemId)
                .ToListAsync();
        }

        public async Task<InvoiceItem?> GetByIdAsync(int id)
        {
            return await _db.InvoiceItems.FirstOrDefaultAsync(x => x.InvoiceItemId == id);
        }

        public async Task<List<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _db.InvoiceItems
                .AsNoTracking()
                .Where(x => x.InvoiceId == invoiceId)
                .OrderBy(x => x.InvoiceItemId)
                .ToListAsync();
        }

        public async Task<bool> InvoiceExistsAsync(int invoiceId)
        {
            return await _db.Invoices.AnyAsync(x => x.InvoiceId == invoiceId);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(InvoiceItem invoiceItem)
        {
            _db.InvoiceItems.Update(invoiceItem);
        }
    }
}
