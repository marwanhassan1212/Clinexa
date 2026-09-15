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
                .OrderBy(x => x.DoctorId)
                .ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await _db.Doctors
                .AsNoTracking()
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
    }
}
