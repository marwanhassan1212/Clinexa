using Clinexa.Data;
using Clinexa.Enums;
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
            var query = _db.Payments
                .AsNoTracking()
                .Include(x => x.Invoice)
                    .ThenInclude(x => x.Patient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    (x.ReferenceNumber != null &&
                     x.ReferenceNumber.Contains(search))
                    ||
                    x.InvoiceId.ToString().Contains(search)
                    ||
                    (x.Invoice.Patient.FirstName + " " +
                     x.Invoice.Patient.LastName).Contains(search));
            }

            if (invoiceId.HasValue)
            {
                query = query.Where(x =>
                    x.InvoiceId == invoiceId.Value);
            }

            if (paymentMethod.HasValue)
            {
                query = query.Where(x =>
                    x.PaymentMethod == paymentMethod.Value);
            }

            if (paymentDateFrom.HasValue)
            {
                var fromDate = paymentDateFrom.Value.Date;

                query = query.Where(x =>
                    x.PaymentDate >= fromDate);
            }

            if (paymentDateTo.HasValue)
            {
                var toDate = paymentDateTo.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.PaymentDate < toDate);
            }

            if (minAmount.HasValue)
            {
                query = query.Where(x =>
                    x.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(x =>
                    x.Amount <= maxAmount.Value);
            }

            var totalCount = await query.CountAsync();

            var payments = await query
                .OrderByDescending(x => x.PaymentDate)
                .ThenByDescending(x => x.PaymentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (payments, totalCount);
        }

        public async Task<List<Payment>> GetAllAsync()
        {
           return await _db.Payments
           .AsNoTracking()
           .Include(x => x.Invoice)
                .ThenInclude(x => x.Patient)
           .OrderByDescending(x => x.PaymentDate)
           .ThenByDescending(x => x.PaymentDate)
           .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _db.Payments
                .AsNoTracking()
                .Include(x => x.Invoice)
                    .ThenInclude(x => x.Patient)
                .FirstOrDefaultAsync(x => x.PaymentId == id);
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
