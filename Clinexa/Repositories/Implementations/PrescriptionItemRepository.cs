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
                  .Include(x => x.Medicine)
                  .FirstOrDefaultAsync(x => x.PrescriptionItemId == id);
        }

        public async Task<List<PrescriptionItem>> GetByPrescriptionIdAsync(int prescriptionId)
        {
            return await _db.PrescriptionItems
               .AsNoTracking()
               .Include(x => x.Medicine)
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

        public async Task<bool> ExistsForPrescriptionAsync(
                  int prescriptionId,
                  int medicineId,
                  int? excludedPrescriptionItemId = null)
        {
            return await _db.PrescriptionItems
                .AnyAsync(x =>
                    x.PrescriptionId == prescriptionId &&
                    x.MedicineId == medicineId &&
                    (!excludedPrescriptionItemId.HasValue ||
                     x.PrescriptionItemId != excludedPrescriptionItemId.Value));
        }

        public async Task<bool> ActiveMedicineExistsAsync(int medicineId)
        {
            return await _db.Medicines
                .AnyAsync(x =>
                    x.MedicineId == medicineId &&
                    x.IsActive);
        }

        public async Task<bool> BelongsToDoctorAsync(
              int prescriptionItemId,
              int doctorUserId)
        {
            return await _db.PrescriptionItems
                .AsNoTracking()
                .AnyAsync(x =>
                    x.PrescriptionItemId ==
                        prescriptionItemId &&
                    x.Prescription.MedicalRecord.Doctor.UserId == doctorUserId);
        }

        public async Task<bool> PrescriptionBelongsToDoctorAsync(int prescriptionId, int doctorUserId)
        {
            return await _db.Prescriptions
                .AsNoTracking()
                .AnyAsync(x =>
                    x.PrescriptionId == prescriptionId &&
                    x.MedicalRecord.Doctor.UserId ==
                        doctorUserId);
        }
    }
}
