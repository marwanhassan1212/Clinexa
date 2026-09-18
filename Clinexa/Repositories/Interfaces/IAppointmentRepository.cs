using Clinexa.Enums;
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

        Task<(List<Appointment> Appointments, int TotalCount)> FilterAsync(string? search, int? doctorId,
             int? patientId, DateTime? appointmentDate, AppointmentStatus? status, int page, int pageSize);
        Task<bool> IsDoctorAvailableAsync(
            int doctorId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime);

        Task<List<DoctorSchedule>> GetDoctorSchedulesAsync(
            int doctorId,
            DayOfWeek dayOfWeek);
        Task<bool> HasConflictAsync(
            int doctorId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludedAppointmentId = null);


        Task<List<Appointment>> GetDoctorAppointmentsAsync(
                int doctorId,
                DateTime appointmentDate);

        Task<Doctor?> GetDoctorWithScheduleAsync(int doctorId);

        Task<List<Appointment>> GetDoctorAppointmentsForWeekAsync(int doctorId, DateTime weekStart,
            DateTime weekEnd);
        


        Task AddAsync(Appointment appointment);

        void Update(Appointment appointment);

        Task SaveChangesAsync();
    }
}
