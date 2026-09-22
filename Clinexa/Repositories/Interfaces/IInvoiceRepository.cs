using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(int id);

        Task<List<Invoice>> GetAllAsync();

        Task<List<Invoice>> GetByPatientIdAsync(int patientId);

        Task<Invoice?> GetByAppointmentIdAsync(int appointmentId);

        Task<bool> PatientExistsAsync(int patientId);


        Task<(List<Invoice> Invoices, int TotalCount)> FilterAsync(
         string? search,
         int? patientId,
         string? invoiceStatus,
         DateTime? dateFrom,
         DateTime? dateTo,
         string sortBy,
         string sortDirection,
         int page,
         int pageSize);
        Task<bool> AppointmentExistsAsync(int appointmentId);

        Task<bool> ExistsForAppointmentAsync(
            int appointmentId,
            int? excludedInvoiceId = null);

        Task AddAsync(Invoice invoice);

        void Update(Invoice invoice);

        Task SaveChangesAsync();
    }
}
