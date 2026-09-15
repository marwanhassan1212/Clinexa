using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient?> GetByIdAsync(int id);

        Task<List<Patient>> GetAllAsync();

        Task<List<Patient>> SearchAsync(string searchTerm);

        Task<bool> CreateAsync(Patient patient);

        Task<bool> UpdateAsync(Patient patient);

        Task<bool> DeactivateAsync(int id);

    }
}
