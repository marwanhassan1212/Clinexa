using Clinexa.Enums;
using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(int id);

        Task<List<Patient>> GetAllAsync();

        Task<List<Patient>> SearchAsync(string searchTerm);
        Task<(List<Patient> Patients, int TotalCount)> FilterAsync(string? search, Gender? gender,
            string? bloodType, bool? isActive, int page, int pageSize);
        Task<bool> ExistsByPhoneAsync(string phoneNumber);

        Task AddAsync(Patient patient);

        void Update(Patient patient);

        Task SaveChangesAsync();
    }
}
