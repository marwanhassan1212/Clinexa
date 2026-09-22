using Clinexa.Data;
using Clinexa.Enums;
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
            return await _db.Doctors
                        .AnyAsync(x => x.DoctorId == doctorId && x.IsActive);
        }

        public async Task<(List<Appointment> Appointments, int TotalCount)> FilterAsync(
                 string? search,
                 int? doctorId,
                 int? patientId,
                 DateTime? appointmentDate,
                 AppointmentStatus? status,
                 int page,
                 int pageSize)
        {
            var query = _db.Appointments
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.Speciality)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    (x.Patient.FirstName + " " + x.Patient.LastName).Contains(search) ||
                    x.Patient.PhoneNumber.Contains(search) ||
                    (x.Doctor.User.FirstName + " " + x.Doctor.User.LastName).Contains(search));
            }

            if (doctorId.HasValue)
            {
                query = query.Where(x => x.DoctorId == doctorId.Value);
            }

            if (patientId.HasValue)
            {
                query = query.Where(x => x.PatientId == patientId.Value);
            }

            if (appointmentDate.HasValue)
            {
                var dayStart = appointmentDate.Value.Date;
                var dayEnd = dayStart.AddDays(1);

                query = query.Where(x =>
                    x.AppointmentDate >= dayStart &&
                    x.AppointmentDate < dayEnd);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.AppointmentStatus == status.Value);
            }

            var totalCount = await query.CountAsync();

            var appointments = await query
                .OrderByDescending(x => x.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (appointments, totalCount);
        }

        public async Task<List<Appointment>> GetAllAsync()
        {
            return await _db.Appointments
                 .AsNoTracking()
                 .OrderByDescending(x => x.AppointmentDate)
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
            return await _db.Appointments
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.Speciality)
                .FirstOrDefaultAsync(x => x.AppointmentId == id);
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

        public async Task<List<Appointment>> GetDoctorAppointmentsAsync(
        int doctorId,
        DateTime appointmentDate)
        {
            var dayStart = appointmentDate.Date;
            var dayEnd = dayStart.AddDays(1);

            return await _db.Appointments
                .AsNoTracking()
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.AppointmentDate >= dayStart &&
                    x.AppointmentDate < dayEnd &&
                    x.AppointmentStatus != AppointmentStatus.Cancelled)
                .ToListAsync();
        }

        public async Task<List<DoctorSchedule>> GetDoctorSchedulesAsync(
             int doctorId,
             DayOfWeek dayOfWeek)
        {
            return await _db.DoctorSchedules
                .AsNoTracking()
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.DayOfWeek == dayOfWeek &&
                    x.IsAvailable)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<bool> HasConflictAsync(int doctorId, DateTime appointmentDate, TimeSpan startTime,
                                 TimeSpan endTime,
                                 int? excludedAppointmentId = null)
        {
            var dayStart = appointmentDate.Date;
            var dayEnd = dayStart.AddDays(1);

            return await _db.Appointments
                .AnyAsync(x =>
                    x.DoctorId == doctorId &&
                    x.AppointmentDate >= dayStart &&
                    x.AppointmentDate < dayEnd &&
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
                .AnyAsync(x => x.PatientId == patientId && x.IsActive);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(Appointment appointment)
        {
            _db.Appointments.Update(appointment);
        }

        public async Task<List<Appointment>> GetDoctorAppointmentsForWeekAsync(
        int doctorId,
        DateTime weekStart,
        DateTime weekEnd)
        {
            return await _db.Appointments
                .AsNoTracking()
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.AppointmentDate >= weekStart &&
                    x.AppointmentDate < weekEnd &&
                    x.AppointmentStatus != AppointmentStatus.Cancelled)
                .OrderBy(x => x.AppointmentDate)
                .ThenBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorWithScheduleAsync(int doctorId)
        {
            return await _db.Doctors
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Speciality)
                .Include(x => x.DoctorSchedules)
                .FirstOrDefaultAsync(x => x.DoctorId == doctorId);
        }
    }
}
