using Clinexa.Enums;
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
        Task<List<TimeSpan>> GetAvailableSlotsAsync(
            int doctorId,
            DateTime appointmentDate);

        Task<(List<Appointment> Appointments, int TotalCount)> FilterAsync(
           string? search,
           int? doctorId,
           int? patientId,
           DateTime? appointmentDate,
           AppointmentStatus? status,
           int page,
           int pageSize);
        Task<bool> UpdateAsync(Appointment appointment);

        Task<bool> CancelAsync(int id , string? cancellationReason);

        Task<bool> ConfirmAsync(int id);

        Task<bool> CheckInAsync(int id);

        Task<bool> StartConsultationAsync(int id);

        Task<bool> CompleteAsync(int id);

        Task<bool> MarkAsNoShowAsync(int id);
    }
}
