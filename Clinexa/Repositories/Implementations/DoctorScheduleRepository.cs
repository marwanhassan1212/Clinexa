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
            return await _db.DoctorSchedules.AnyAsync(x => x.DoctorId == doctorId);
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
            return await _db.DoctorSchedules.FirstOrDefaultAsync(x => x.DoctorScheduleId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(DoctorSchedule schedule)
        {
            _db.DoctorSchedules.Update(schedule);
        }
    }
}
