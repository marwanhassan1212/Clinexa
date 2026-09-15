using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(int id);

        Task<List<Patient>> GetAllAsync();

        Task<List<Patient>> SearchAsync(string searchTerm);

        Task<bool> ExistsByPhoneAsync(string phoneNumber);

        Task AddAsync(Patient patient);

        void Update(Patient patient);

        Task SaveChangesAsync();
    }
}
