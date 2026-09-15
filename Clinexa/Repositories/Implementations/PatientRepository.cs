using Clinexa.Data;
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
    }
}
