using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Clinexa.Repositories.Implementations
{
    public class SpecialityRepository : ISpecialityRepository
    {
        private readonly AppDbContext _db;
        public SpecialityRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Speciality speciality)
        {
            await _db.Specialities.AddAsync(speciality);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _db.Specialities
                 .AnyAsync(x => x.Name == name);
        }

        public async Task<List<Speciality>> GetAllAsync()
        {
            return await _db.Specialities
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Speciality?> GetByIdAsync(int id)
        {
            return await _db.Specialities
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SpecialityId == id);
        }

        public async Task SaveChangesAsync()
        {
             await _db.SaveChangesAsync();
        }


        void ISpecialityRepository.Update(Speciality speciality)
        {
            _db.Specialities.Update(speciality);
        }
    }
}
