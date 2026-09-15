using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<Appointment?> GetByIdAsync(int id);

        Task<List<Appointment>> GetAllAsync();

        Task<List<Appointment>> GetByDoctorIdAsync(int doctorId);

        Task<List<Appointment>> GetByPatientIdAsync(int patientId);

        Task<bool> CreateAsync(Appointment appointment);

        Task<bool> UpdateAsync(Appointment appointment);

        Task<bool> CancelAsync(int id);
    }
}
