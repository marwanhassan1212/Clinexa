using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class DoctorScheduleRepository : IDoctorScheduleRepository
    {
        private readonly AppDbContext _db;
        public DoctorScheduleRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(DoctorSchedule schedule)
        {
            await _db.DoctorSchedules.AddAsync(schedule);
        }

        public async Task<bool> DoctorExistsAsync(int doctorId)
        {
            return await _db.Doctors
                 .AnyAsync(x =>
                     x.DoctorId == doctorId &&
                     x.IsActive);
        }

        public async Task<bool> ExistsAsync(int doctorId, DayOfWeek dayOfWeek, TimeSpan startTime)
        {
            return await _db.DoctorSchedules
                .AnyAsync(x => x.DoctorId == doctorId
                && x.DayOfWeek == dayOfWeek
                && x.StartTime == startTime);
                
        }

        public async Task<List<DoctorSchedule>> GetAllAsync()
        {
            return await _db.DoctorSchedules
               .AsNoTracking()
               .Include(x => x.Doctor)
                   .ThenInclude(x => x.User)
               .Include(x => x.Doctor)
                   .ThenInclude(x => x.Speciality)
               .OrderBy(x => x.DoctorId)
               .ThenBy(x => x.DayOfWeek)
               .ThenBy(x => x.StartTime)
               .ToListAsync();

        }

        public async Task<List<DoctorSchedule>> GetByDoctorIdAsync(int doctorId)
        {
            return await _db.DoctorSchedules
                .AsNoTracking()
                .Where(x => x.DoctorId == doctorId)
                .OrderBy(x => x.DoctorId)
                .ThenBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<DoctorSchedule?> GetByIdAsync(int id)
        {
            return await _db.DoctorSchedules
                .AsNoTracking()
                 .Include(x => x.Doctor)
                   .ThenInclude(x => x.User)
               .Include(x => x.Doctor)
                   .ThenInclude(x => x.Speciality)
                   .FirstOrDefaultAsync(x => x.DoctorScheduleId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(DoctorSchedule schedule)
        {
            _db.DoctorSchedules.Update(schedule);
        }

        public async Task<(List<DoctorSchedule> Schedules, int TotalCount)> FilterAsync(
                string? search,
                int? doctorId,
                DayOfWeek? dayOfWeek,
                bool? isAvailable,
                int page,
                int pageSize)
        {
            var query = _db.DoctorSchedules
                .AsNoTracking()
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.Speciality)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    (x.Doctor.User.FirstName + " " + x.Doctor.User.LastName)
                        .Contains(search));
            }

            if (doctorId.HasValue)
            {
                query = query.Where(x =>
                    x.DoctorId == doctorId.Value);
            }

            if (dayOfWeek.HasValue)
            {
                query = query.Where(x =>
                    x.DayOfWeek == dayOfWeek.Value);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(x =>
                    x.IsAvailable == isAvailable.Value);
            }

            var totalCount = await query.CountAsync();

            var schedules = await query
                .OrderBy(x => x.Doctor.User.FirstName)
                .ThenBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (schedules, totalCount);
        }

        public async Task<bool> HasOverlapAsync(int doctorId, DayOfWeek dayOfWeek,
        TimeSpan startTime, TimeSpan endTime, int? excludedScheduleId = null)
        {
            var query = _db.DoctorSchedules
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.DayOfWeek == dayOfWeek &&
                    x.IsAvailable &&
                    x.StartTime < endTime &&
                    x.EndTime > startTime);

            if (excludedScheduleId.HasValue)
            {
                query = query.Where(x =>
                    x.DoctorScheduleId != excludedScheduleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasDuplicateStartTimeAsync(int doctorId, DayOfWeek dayOfWeek,
        TimeSpan startTime, int? excludedScheduleId = null)
        {
            var query = _db.DoctorSchedules
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.DayOfWeek == dayOfWeek &&
                    x.StartTime == startTime);

            if (excludedScheduleId.HasValue)
            {
                query = query.Where(x =>
                    x.DoctorScheduleId != excludedScheduleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task DeactivateByDoctorIdAsync(int doctorId)
        {
            var schedules = await _db.DoctorSchedules
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.IsAvailable)
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                schedule.IsAvailable = false;
            }
        }

        public async Task ActivateByDoctorIdAsync(int doctorId)
        {
            var schedules = await _db.DoctorSchedules
                .Where(x =>
                    x.DoctorId == doctorId &&
                    !x.IsAvailable)
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                schedule.IsAvailable = true;
            }
        }
    }
}
