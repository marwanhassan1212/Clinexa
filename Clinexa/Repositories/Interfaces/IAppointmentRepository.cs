using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(int id);

        Task<List<Appointment>> GetAllAsync();

        Task<List<Appointment>> GetByDoctorIdAsync(int doctorId);

        Task<List<Appointment>> GetByPatientIdAsync(int patientId);

        Task<bool> DoctorExistsAsync(int doctorId);

        Task<bool> PatientExistsAsync(int patientId);

        Task<bool> IsDoctorAvailableAsync(
            int doctorId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime);

        Task<bool> HasConflictAsync(
            int doctorId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludedAppointmentId = null);

        Task AddAsync(Appointment appointment);

        void Update(Appointment appointment);

        Task SaveChangesAsync();
    }
}
