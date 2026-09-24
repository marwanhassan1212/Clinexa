using Clinexa.Data;
using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _db;
        public InvoiceRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _db.Invoices.AddAsync(invoice);
        }

        public async Task<bool> AppointmentExistsAsync(int appointmentId)
        {
            return await _db.Appointments.AnyAsync(x => x.AppointmentId == appointmentId);
        }

        public async Task<bool> ExistsForAppointmentAsync(int appointmentId, int? excludedInvoiceId = null)
        {
            return await _db.Invoices
          .AnyAsync(x =>
              x.AppointmentId == appointmentId &&
              (!excludedInvoiceId.HasValue ||
               x.InvoiceId != excludedInvoiceId.Value));
        }

        public async Task<(List<Invoice> Invoices, int TotalCount)> FilterAsync(string? search, int? patientId, string? invoiceStatus, DateTime? dateFrom, DateTime? dateTo, string sortBy, string sortDirection, int page, int pageSize)
        {
            var query = _db.Invoices
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Appointment)
            .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                if (int.TryParse(search, out int invoiceId))
                {
                    query = query.Where(x =>
                        x.InvoiceId == invoiceId ||
                        x.Patient.FirstName.Contains(search) ||
                        x.Patient.LastName.Contains(search));
                }
                else
                {
                    query = query.Where(x =>
                        x.Patient.FirstName.Contains(search) ||
                        x.Patient.LastName.Contains(search));
                }
            }

            // Patient filter
            if (patientId.HasValue)
            {
                query = query.Where(x =>
                    x.PatientId == patientId.Value);
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(invoiceStatus) &&
                Enum.TryParse<InvoiceStatus>(
                    invoiceStatus,
                    true,
                    out var status))
            {
                query = query.Where(x =>
                    x.InvoiceStatus == status);
            }

            // Date From
            if (dateFrom.HasValue)
            {
                query = query.Where(x =>
                    x.InvoiceDate >= dateFrom.Value);
            }

            // Date To
            if (dateTo.HasValue)
            {
                var nextDay = dateTo.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.InvoiceDate < nextDay);
            }

            // Sorting
            bool descending =
                string.Equals(
                    sortDirection,
                    "Desc",
                    StringComparison.OrdinalIgnoreCase);

            query = sortBy?.ToLower() switch
            {
                "total" => descending
                    ? query.OrderByDescending(x => x.TotalAmount)
                    : query.OrderBy(x => x.TotalAmount),

                "status" => descending
                    ? query.OrderByDescending(x => x.InvoiceStatus)
                    : query.OrderBy(x => x.InvoiceStatus),

                "patient" => descending
                    ? query.OrderByDescending(
                        x => x.Patient.LastName)
                    : query.OrderBy(
                        x => x.Patient.LastName),

                _ => descending
                    ? query.OrderByDescending(x => x.InvoiceDate)
                    : query.OrderBy(x => x.InvoiceDate)
            };

            // Total after filters
            var totalCount = await query.CountAsync();

            // Pagination
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var invoices = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (invoices, totalCount);
        }


        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _db.Invoices
                .AsNoTracking()
                .OrderByDescending(x => x.InvoiceDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _db.Invoices.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _db.Invoices
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Appointment)
                    .ThenInclude(x => x.Doctor)
                        .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.InvoiceId == id);
        }

        public async Task<List<Invoice>> GetByPatientIdAsync(int patientId)
        {
            return await _db.Invoices
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.InvoiceDate)
                .ToListAsync();
        }

        public async Task<bool> IsAppointmentCancelledAsync(int appointmentId)
        {
            return await _db.Appointments
                .AsNoTracking()
                .AnyAsync(x =>
                    x.AppointmentId == appointmentId &&
                    x.AppointmentStatus == AppointmentStatus.Cancelled);
        }

        public async Task<bool> PatientExistsAsync(int patientId)
        {
            return await _db.Patients.AnyAsync(x => x.PatientId == patientId);
                
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(Invoice invoice)
        {
            _db.Invoices.Update(invoice);
        }

      
    }
}
