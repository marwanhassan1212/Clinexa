using Clinexa.Data;
using Clinexa.Enums;
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
           .Include(x => x.MedicalRecord)
               .ThenInclude(x => x.Patient)

           .Include(x => x.MedicalRecord)
               .ThenInclude(x => x.Doctor)
                   .ThenInclude(x => x.User)

           .Include(x => x.MedicalRecord)
               .ThenInclude(x => x.Appointment)

           .Include(x => x.PrescriptionItems)
               .ThenInclude(x => x.Medicine)

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

        public async Task<(List<Prescription> Prescriptions, int TotalCount)> FilterAsync(
         string? search,
         DateTime? dateFrom,
         DateTime? dateTo,
         int? medicalRecordId,
         string sortBy,
         string sortDirection,
         int page,
         int pageSize)

        {
            var query = _db.Prescriptions
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                if (int.TryParse(search, out int prescriptionId))
                {
                    query = query.Where(x =>
                        x.PrescriptionId == prescriptionId ||
                        (x.Notes != null &&
                         x.Notes.Contains(search)));
                }
                else
                {
                    query = query.Where(x =>
                        x.Notes != null &&
                        x.Notes.Contains(search));
                }
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(x =>
                    x.PrescriptionDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(x =>
                    x.PrescriptionDate <= dateTo.Value);
            }

            if (medicalRecordId.HasValue)
            {
                query = query.Where(x =>
                    x.MedicalRecordId == medicalRecordId.Value);
            }

            var totalCount = await query.CountAsync();

            query = sortBy switch
            {
                "Id" => sortDirection == "Asc"
                    ? query.OrderBy(x => x.PrescriptionId)
                    : query.OrderByDescending(x => x.PrescriptionId),

                "Date" => sortDirection == "Asc"
                    ? query.OrderBy(x => x.PrescriptionDate)
                    : query.OrderByDescending(x => x.PrescriptionDate),

                "MedicalRecord" => sortDirection == "Asc"
                    ? query.OrderBy(x => x.MedicalRecordId)
                    : query.OrderByDescending(x => x.MedicalRecordId),

                _ => query.OrderByDescending(x => x.PrescriptionDate)
            };

            var prescriptions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (prescriptions, totalCount);
        }

        public async Task<List<MedicalRecord>> GetMedicalRecordsAsync()
        {
            return await _db.MedicalRecords
                .AsNoTracking()
                .OrderByDescending(x => x.MedicalRecordId)
                .ToListAsync();
        }

        public async Task<List<MedicalRecord>> SearchMedicalRecordsAsync(string? search, int take = 10)
        {
            IQueryable<MedicalRecord> query = _db.MedicalRecords
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                .Include(x => x.Appointment);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    EF.Functions.Like(
                        x.Patient.FirstName,
                        $"%{search}%")

                    || EF.Functions.Like(
                        x.Patient.LastName,
                        $"%{search}%")

                    || EF.Functions.Like(
                        x.Doctor.User.FirstName,
                        $"%{search}%")

                    || EF.Functions.Like(
                        x.Doctor.User.LastName,
                        $"%{search}%")

                    || x.MedicalRecordId.ToString().Contains(search)
                );
            }

            return await query
                .OrderByDescending(x => x.MedicalRecordId)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> IsMedicalRecordAppointmentCompletedAsync(int medicalRecordId)
        {
            return await _db.MedicalRecords
                .AsNoTracking()
                .AnyAsync(x =>
                    x.MedicalRecordId == medicalRecordId &&
                    x.Appointment.AppointmentStatus == AppointmentStatus.Completed);
        }
    }
}
