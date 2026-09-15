using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<bool> CreateAsync(Invoice invoice);

        Task<List<Invoice>> GetAllAsync();

        Task<Invoice?> GetByIdAsync(int id);

        Task<List<Invoice>> GetByPatientIdAsync(int patientId);

        Task<Invoice?> GetByAppointmentIdAsync(int appointmentId);

        Task<bool> UpdateAsync(Invoice invoice);
    }
}
