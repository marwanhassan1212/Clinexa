using Clinexa.Data;
using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Clinexa.Repositories.Implementations
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _db;
        public PatientRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(Patient patient)
        {
            await _db.Patients.AddAsync(patient);
        }

        public async Task<bool> ExistsByPhoneAsync(string phoneNumber)
        {
            return await _db.Patients.AnyAsync(x => x.PhoneNumber == phoneNumber);
        }

        public async Task<List<Patient>> GetAllAsync()
        {
            return await _db.Patients.AsNoTracking()
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _db.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == id);
        }

        public async Task SaveChangesAsync()
        {
             await _db.SaveChangesAsync();
        }

        public async Task<List<Patient>> SearchAsync(string searchTerm)
        {
            return await _db.Patients
                .AsNoTracking()
                .Where(x => x.FirstName.Contains(searchTerm)
                || x.LastName.Contains(searchTerm)
                || x.PhoneNumber.Contains(searchTerm))
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToListAsync();
                
        }

        public void Update(Patient patient)
        {
            _db.Patients.Update(patient);
        }

        public async Task<(List<Patient> Patients, int TotalCount)> FilterAsync(string? search,
             Gender? gender, string? bloodType, bool? isActive, int page, int pageSize)
        {
            var query = _db.Patients
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.FirstName.Contains(search) ||
                    x.LastName.Contains(search) ||
                    x.PhoneNumber.Contains(search) ||
                    (x.Email != null && x.Email.Contains(search)));
            }

            if (gender.HasValue)
            {
                query = query.Where(x => x.Gender == gender.Value);
            }

            if (!string.IsNullOrWhiteSpace(bloodType))
            {
                query = query.Where(x => x.BloodType == bloodType);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var patients = await query
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (patients, totalCount);
        }
    }
}
