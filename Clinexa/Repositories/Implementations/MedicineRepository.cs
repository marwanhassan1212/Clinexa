using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly AppDbContext _db;
        public MedicineRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(Medicine medicine)
        {
            await _db.Medicines.AddAsync(medicine);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludedMedicineId = null)
        {
            return await _db.Medicines
                    .AnyAsync(x =>
                     x.Name == name &&
                    (!excludedMedicineId.HasValue ||
                     x.MedicineId !=
                    excludedMedicineId.Value));
        }

        public async Task<List<Medicine>> GetActiveAsync()
        {
            return await _db.Medicines
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
        }

        public async Task<List<Medicine>> GetAllAsync()
        {
            return await _db.Medicines
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Medicine?> GetByIdAsync(int id)
        {
            return await _db.Medicines
                .FirstOrDefaultAsync(x => x.MedicineId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(Medicine medicine)
        {
            _db.Medicines.Update(medicine);
        }

        public async Task<(List<Medicine> Medicines, int TotalCount)> FilterAsync(
                 string? search,
                 bool? isActive,
                 int page,
                 int pageSize)
        {
            var query = _db.Medicines
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    (x.GenericName != null &&
                     x.GenericName.Contains(search)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x =>
                    x.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var medicines = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (medicines, totalCount);
        }
    }
}
