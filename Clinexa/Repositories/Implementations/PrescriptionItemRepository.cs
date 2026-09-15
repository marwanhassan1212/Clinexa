using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class PrescriptionItemRepository : IPrescriptionItemRepository
    {
        private readonly AppDbContext _db;
        public PrescriptionItemRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(PrescriptionItem prescriptionItem)
        {
            await _db.PrescriptionItems.AddAsync(prescriptionItem);
        }

        public async Task<List<PrescriptionItem>> GetAllAsync()
        {
            return await _db.PrescriptionItems
                 .AsNoTracking()
                 .OrderByDescending(x => x.PrescriptionItemId)
                 .ToListAsync();
        }

        public async Task<PrescriptionItem?> GetByIdAsync(int id)
        {
            return await _db.PrescriptionItems
                .FirstOrDefaultAsync(x => x.PrescriptionItemId == id);
        }

        public async Task<List<PrescriptionItem>> GetByPrescriptionIdAsync(int prescriptionId)
        {
            return await _db.PrescriptionItems
                .AsNoTracking()
                .Where(x => x.PrescriptionId == prescriptionId)
                .OrderBy(x => x.PrescriptionItemId)
                .ToListAsync();   
        }

        public async Task<bool> MedicineExistsAsync(int medicineId)
        {
            return await _db.Medicines.AnyAsync(x => x.MedicineId == medicineId);
        }

        public async Task<bool> PrescriptionExistsAsync(int prescriptionId)
        {
            return await _db.Prescriptions.AnyAsync(x => x.PrescriptionId == prescriptionId);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(PrescriptionItem prescriptionItem)
        {
            _db.PrescriptionItems.Update(prescriptionItem);
        }
    }
}
