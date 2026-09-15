using Clinexa.Data;
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
            return await _db.Invoices.FirstOrDefaultAsync(x => x.InvoiceId == id);
        }

        public async Task<List<Invoice>> GetByPatientIdAsync(int patientId)
        {
            return await _db.Invoices
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.InvoiceDate)
                .ToListAsync();
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
