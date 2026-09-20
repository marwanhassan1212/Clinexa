using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _db;
        public DoctorRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(Doctor doctor)
        {
            await _db.Doctors.AddAsync(doctor);
        }

        public async Task<bool> ExistBySpecialityId(int id)
        {
            return await _db.Specialities
                .AnyAsync(x => x.SpecialityId == id);
        }

        public async Task<bool> ExistsByUserId(int id)
        {
            return await _db.Doctors
                .AnyAsync(x => x.UserId == id);
                
        }

        public async Task<List<Doctor>> GetAllAsync()
        {
            return await _db.Doctors
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Speciality)
                .OrderBy(x => x.DoctorId)
                .ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await _db.Doctors
                
                .Include(x => x.User)
                .Include(x => x.Speciality)
                .FirstOrDefaultAsync(x => x.DoctorId == id);
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }

        public void UpdateAsync(Doctor doctor)
        {
            _db.Doctors.Update(doctor);
        }

        public async Task<(List<Doctor> Doctors, int TotalCount)> FilterAsync(
       string? search,
       int? specialityId,
       bool? isActive,
       int page,
       int pageSize)
        {
            var query = _db.Doctors
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Speciality)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.User.FirstName.Contains(search) ||
                    x.User.LastName.Contains(search) ||
                    x.User.Email.Contains(search) ||
                    x.User.PhoneNumber.Contains(search));
            }

            if (specialityId.HasValue)
            {
                query = query.Where(x =>
                    x.SpecialityId == specialityId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x =>
                    x.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var doctors = await query
                .OrderBy(x => x.User.FirstName)
                .ThenBy(x => x.User.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (doctors, totalCount);
        }

        public async Task<List<User>> GetAvailableUsersAsync()
        {
            return await _db.Users
                .AsNoTracking()
                .Include(x => x.Role)
                .Where(x =>
                    x.Role.Name == "Doctor" &&
                    x.Doctor == null &&
                    x.IsActive)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToListAsync();
        }
    }
}
