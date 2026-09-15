using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly AppDbContext _db;
        public PrescriptionRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(Prescription prescription)
        {
            await _db.Prescriptions.AddAsync(prescription);
        }

        public async Task<bool> ExistsForMedicalRecordAsync(int medicalRecordId, int? excludedPrescriptionId = null)
        {
            return await _db.Prescriptions
               .AnyAsync(x =>
                   x.MedicalRecordId == medicalRecordId &&
                   (!excludedPrescriptionId.HasValue ||
                    x.PrescriptionId !=
                    excludedPrescriptionId.Value));
        }

        public async Task<List<Prescription>> GetAllAsync()
        {
            return await _db.Prescriptions
                .AsNoTracking()
                .OrderByDescending(x => x.PrescriptionDate)
                .ToListAsync();
        }

        public async Task<Prescription?> GetByIdAsync(int id)
        {
            return await _db.Prescriptions
                .FirstOrDefaultAsync(x => x.PrescriptionId == id);
        }

        public async Task<Prescription?> GetByMedicalRecordIdAsync(int medicalRecordId)
        {
            return await _db.Prescriptions
                .FirstOrDefaultAsync(x => x.MedicalRecordId == medicalRecordId);
        }

        public async Task<bool> MedicalRecordExistsAsync(int medicalRecordId)
        {
            return await _db.MedicalRecords
                .AnyAsync(x => x.MedicalRecordId == medicalRecordId);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(Prescription prescription)
        {
            _db.Prescriptions.Update(prescription);
        }
    }
}
