using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _db;
        public AppointmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Appointment appointment)
        {
            await _db.Appointments.AddAsync(appointment);
        }

        public async Task<bool> DoctorExistsAsync(int doctorId)
        {
            return await _db.Doctors.AnyAsync(x => x.DoctorId == doctorId);
        }

        public async Task<List<Appointment>> GetAllAsync()
        {
            return await _db.Appointments
                 .AsNoTracking()
                 .OrderBy(x => x.AppointmentDate)
                 .ThenBy(x => x.StartTime)
                 .ToListAsync();
        }

        public async Task<List<Appointment>> GetByDoctorIdAsync(int doctorId)
        {
            return await _db.Appointments
                .AsNoTracking()
                .Where(x => x.DoctorId == doctorId)
                .OrderBy(x => x.AppointmentDate)
                .ThenBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _db.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == id);
        }

        public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
        {
            return await _db.Appointments
                .AsNoTracking()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.AppointmentDate)
                .ThenByDescending(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime, int? excludedAppointmentId = null)
        {
            return await _db.Appointments
              .AnyAsync(x =>
                  x.DoctorId == doctorId &&
                  x.AppointmentDate.Date == appointmentDate.Date &&
                  x.StartTime < endTime &&
                  x.EndTime > startTime &&
                  (!excludedAppointmentId.HasValue ||
                   x.AppointmentId != excludedAppointmentId.Value));
        }

        public async Task<bool> IsDoctorAvailableAsync(int doctorId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            return await _db.DoctorSchedules
                .AnyAsync(x => x.DoctorId == doctorId
                && x.DayOfWeek == dayOfWeek
                && x.IsAvailable
                && x.StartTime <= startTime
                && x.EndTime >= endTime);
        }

        public async Task<bool> PatientExistsAsync(int patientId)
        {
            return await _db.Patients
                .AnyAsync(x => x.PatientId == patientId);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(Appointment appointment)
        {
            _db.Appointments.Update(appointment);
        }
    }
}
