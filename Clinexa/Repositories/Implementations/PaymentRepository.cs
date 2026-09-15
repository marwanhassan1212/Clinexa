using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {

        private readonly AppDbContext _db;
        public PaymentRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(Payment payment)
        {
            await _db.Payments.AddAsync(payment);
        }

        public async Task<List<Payment>> GetAllAsync()
        {
           return await _db.Payments
           .AsNoTracking()
           .OrderByDescending(x => x.PaymentDate)
           .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _db.Payments.FirstOrDefaultAsync(x => x.PaymentId == id);
        }

        public async Task<List<Payment>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _db.Payments
                .AsNoTracking()
                .Where(x => x.InvoiceId == invoiceId)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _db.Invoices.FirstOrDefaultAsync(x => x.InvoiceId == invoiceId);
        }

        public async Task<decimal> GetTotalPaidForInvoiceAsync(int invoiceId)
        {
            return await _db.Payments
              .Where(x => x.InvoiceId == invoiceId)
              .SumAsync(x => x.Amount);
        }

        public async Task<bool> InvoiceExistsAsync(int invoiceId)
        {
            return await _db.Invoices.AnyAsync(x => x.InvoiceId == invoiceId);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(Payment payment)
        {
            _db.Payments.Update(payment);
        }
    }
}
